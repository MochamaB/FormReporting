1. Submission Statistics ✅ Already Possible
Data source: FormTemplateSubmission table

Reports you can build:

Total submissions per template
Submissions by status (Draft, Submitted, Approved, Rejected)
On-time vs late submissions (compare SubmittedDate to Period.EndDate)
Submission rate by tenant/department/user
Average completion time (CreatedDate to SubmittedDate)

2. Field Scoring ✅ Already Possible
Data source: FormTemplateResponse table

You already store:

SelectedScoreValue (from option)
SelectedScoreWeight (from option)
WeightedScore (calculated)
Reports you can build:

Individual field scores per submission
Which options were selected most frequently
Average score per field across all submissions
No metrics needed - the scores are already calculated and stored!

3. Section Scoring ✅ Already Possible
Data source: FormTemplateResponse + FormTemplateSection

How to calculate:

Get all responses for fields in a section
Average the WeightedScore values
OR sum them depending on your requirement
Example logic:

Section: "Customer Satisfaction" (3 fields)
Submission #123:
  - Field 1 (Response Time): WeightedScore = 4.0
  - Field 2 (Quality): WeightedScore = 5.0
  - Field 3 (Communication): WeightedScore = 4.5
 
Section Score = (4.0 + 5.0 + 4.5) / 3 = 4.5
 
OR if fields have weights:
Section Score = (4.0 × 0.3) + (5.0 × 0.4) + (4.5 ×

4. Template Overall Score ✅ Already Possible
Data source: Aggregate all section scores

How to calculate:

Calculate each section's average score
Average all section scores
OR use weighted average if sections have importance weights

5. Comparative Reports ✅ Already Possible
Cross-submission comparisons:

Compare this month's scores vs last month
Compare tenant A vs tenant B
Identify lowest scoring sections
Find fields with most "poor" ratings
All from existing FormTemplateResponse data!