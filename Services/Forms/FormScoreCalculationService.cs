using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for calculating scores from form responses
    /// Handles field, section, and template score calculations with weighted averages
    /// </summary>
    public class FormScoreCalculationService : IFormScoreCalculationService
    {
        private readonly ApplicationDbContext _context;

        public FormScoreCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal?> GetFieldAverageScoreAsync(int itemId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.FormTemplateResponses
                .Include(r => r.Submission)
                .Where(r => r.ItemId == itemId && r.WeightedScore.HasValue);

            if (templateId.HasValue)
            {
                query = query.Where(r => r.Submission.TemplateId == templateId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.Submission.SubmittedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.Submission.SubmittedDate <= endDate.Value);
            }

            var scores = await query.Select(r => r.WeightedScore!.Value).ToListAsync();

            return scores.Any() ? scores.Average() : null;
        }

        public async Task<decimal?> GetFieldScoreForSubmissionAsync(int submissionId, int itemId)
        {
            var response = await _context.FormTemplateResponses
                .Where(r => r.SubmissionId == submissionId && r.ItemId == itemId)
                .FirstOrDefaultAsync();

            return response?.WeightedScore;
        }

        public async Task<decimal?> GetSectionScoreAsync(int submissionId, int sectionId)
        {
            var fieldScores = await _context.FormTemplateResponses
                .Include(r => r.Item)
                .Where(r => r.SubmissionId == submissionId 
                         && r.Item.SectionId == sectionId 
                         && r.WeightedScore.HasValue)
                .Select(r => new
                {
                    Score = r.WeightedScore!.Value,
                    Weight = r.Item.Weight
                })
                .ToListAsync();

            if (!fieldScores.Any())
                return null;

            var totalWeight = fieldScores.Sum(f => f.Weight);
            
            if (totalWeight == 0)
                return null;

            var weightedSum = fieldScores.Sum(f => f.Score * f.Weight);

            return weightedSum / totalWeight;
        }

        public async Task<decimal?> GetSectionAverageScoreAsync(int sectionId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.FormTemplateSubmissions.AsQueryable();

            if (templateId.HasValue)
            {
                query = query.Where(s => s.TemplateId == templateId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate <= endDate.Value);
            }

            var submissionIds = await query.Select(s => s.SubmissionId).ToListAsync();

            if (!submissionIds.Any())
                return null;

            var sectionScores = new List<decimal>();

            foreach (var submissionId in submissionIds)
            {
                var score = await GetSectionScoreAsync(submissionId, sectionId);
                if (score.HasValue)
                {
                    sectionScores.Add(score.Value);
                }
            }

            return sectionScores.Any() ? sectionScores.Average() : null;
        }

        public async Task<decimal?> GetTemplateOverallScoreAsync(int submissionId)
        {
            var sectionScores = await _context.FormTemplateSections
                .Where(s => s.Items.Any(i => i.Responses.Any(r => r.SubmissionId == submissionId)))
                .Select(s => new
                {
                    SectionId = s.SectionId,
                    Weight = s.Weight
                })
                .ToListAsync();

            if (!sectionScores.Any())
                return null;

            var weightedScores = new List<(decimal Score, decimal Weight)>();

            foreach (var section in sectionScores)
            {
                var score = await GetSectionScoreAsync(submissionId, section.SectionId);
                if (score.HasValue)
                {
                    weightedScores.Add((score.Value, section.Weight));
                }
            }

            if (!weightedScores.Any())
                return null;

            var totalWeight = weightedScores.Sum(ws => ws.Weight);
            
            if (totalWeight == 0)
                return null;

            var weightedSum = weightedScores.Sum(ws => ws.Score * ws.Weight);

            return weightedSum / totalWeight;
        }

        public async Task<decimal?> GetTemplateAverageScoreAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId && s.Status == "Submitted");

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate <= endDate.Value);
            }

            var submissionIds = await query.Select(s => s.SubmissionId).ToListAsync();

            if (!submissionIds.Any())
                return null;

            var templateScores = new List<decimal>();

            foreach (var submissionId in submissionIds)
            {
                var score = await GetTemplateOverallScoreAsync(submissionId);
                if (score.HasValue)
                {
                    templateScores.Add(score.Value);
                }
            }

            return templateScores.Any() ? templateScores.Average() : null;
        }

        public async Task<SubmissionScoreBreakdownViewModel> GetSubmissionScoreBreakdownAsync(int submissionId)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Item)
                        .ThenInclude(i => i.Section)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                throw new ArgumentException($"Submission {submissionId} not found");

            var breakdown = new SubmissionScoreBreakdownViewModel
            {
                SubmissionId = submissionId,
                TemplateId = submission.TemplateId,
                TemplateName = submission.Template.TemplateName,
                SubmittedDate = submission.SubmittedDate,
                SectionScores = new List<SectionScoreViewModel>(),
                FieldScores = new List<FieldScoreViewModel>()
            };

            var sections = await _context.FormTemplateSections
                .Where(s => s.TemplateId == submission.TemplateId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            foreach (var section in sections)
            {
                var sectionScore = await GetSectionScoreAsync(submissionId, section.SectionId);
                
                breakdown.SectionScores.Add(new SectionScoreViewModel
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    Score = sectionScore,
                    Weight = section.Weight
                });

                var fieldScores = await _context.FormTemplateResponses
                    .Include(r => r.Item)
                    .Where(r => r.SubmissionId == submissionId && r.Item.SectionId == section.SectionId)
                    .OrderBy(r => r.Item.DisplayOrder)
                    .ToListAsync();

                foreach (var response in fieldScores)
                {
                    breakdown.FieldScores.Add(new FieldScoreViewModel
                    {
                        ItemId = response.ItemId,
                        ItemName = response.Item.ItemName,
                        SectionId = section.SectionId,
                        SectionName = section.SectionName,
                        Score = response.WeightedScore,
                        Weight = response.Item.Weight
                    });
                }
            }

            breakdown.OverallScore = await GetTemplateOverallScoreAsync(submissionId);

            return breakdown;
        }

        public async Task<FieldPerformanceViewModel> GetFieldPerformanceAsync(int itemId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var item = await _context.FormTemplateItems
                .Include(i => i.Section)
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
                throw new ArgumentException($"Field {itemId} not found");

            var query = _context.FormTemplateResponses
                .Include(r => r.Submission)
                .Where(r => r.ItemId == itemId);

            if (templateId.HasValue)
            {
                query = query.Where(r => r.Submission.TemplateId == templateId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.Submission.SubmittedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.Submission.SubmittedDate <= endDate.Value);
            }

            var responses = await query.ToListAsync();

            var scoresWithValues = responses.Where(r => r.WeightedScore.HasValue).ToList();

            return new FieldPerformanceViewModel
            {
                ItemId = itemId,
                ItemName = item.ItemName,
                SectionName = item.Section.SectionName,
                ResponseCount = responses.Count,
                AverageScore = scoresWithValues.Any() ? scoresWithValues.Average(r => r.WeightedScore!.Value) : null,
                MinScore = scoresWithValues.Any() ? scoresWithValues.Min(r => r.WeightedScore!.Value) : null,
                MaxScore = scoresWithValues.Any() ? scoresWithValues.Max(r => r.WeightedScore!.Value) : null,
                Weight = item.Weight
            };
        }
    }
}
