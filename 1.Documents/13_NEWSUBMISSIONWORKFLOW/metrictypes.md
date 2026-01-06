✅ Suggested Metric Subcategories (by Category)
🟡 1) Score

Used where the field or section produces a numerical or weighted score.

Possible subcategories

Weighted Score

Normalized Score (0–100)

Threshold Score (Pass/Fail)

Risk Score

Health Score

Quality Score

Composite Score (multi-field)

Section Score Contribution

Template Score Contribution

Example metric definitions

Network Uptime Score (Weighted %)

Overall Infrastructure Health Score

SLA Compliance Score

🟢 2) Performance

Used when measuring how well a process, system, or entity performs.

Subcategories

Reliability

Stability

Efficiency

Capacity Utilization

Success Rate

Failure Rate

Error Frequency

Response Quality

Examples

Network Reliability Index

Incident Failure Rate

Successful Connection Rate

📈 3) Trends

Used for time-series and progression metrics.

Subcategories

Monthly Trend

Weekly Trend

Year-to-Date Trend

Moving Average

Growth Rate

Improvement / Decline Rate

Seasonal Pattern

Period-over-Period Change

Examples

Network Uptime Trend (Monthly)

Incident Reduction Trend

Score Improvement Over Time

⚖ 4) Comparison

Used when comparing against something else.

Subcategories

Benchmark Comparison

Peer Comparison

Department Comparison

Historical Comparison

Version Comparison

Target vs Actual

Best vs Worst Performing

Rank / Position

Examples

Uptime vs SLA Target

Network Reliability Compared to Regions

Current Period vs Previous Period Uptime

⏳ 5) Time

Used when metrics relate to duration or temporal behavior.

Subcategories

Average Time

Total Time

Minimum Time

Maximum Time

Time to Resolution

Time in State

Downtime Duration

Service Availability Duration

Examples

Average Network Uptime Duration

Total Downtime This Month

Mean Time Between Failures (MTBF)

Mean Time To Recover (MTTR)

👉 Your example fits here:

Average Time of Network Uptime
→ Category: Time
→ Subcategory: Average Time

🔢 6) Count

Used when the metric is a frequency or total quantity.

Subcategories

Total Count

Unique Count

Event Count

Failure Count

Alert Count

Incident Count

Occurrence Rate

Repetition Frequency

Examples

Number of Network Outages

Number of Downtime Events

Count of Successful Pings

🛡 7) Compliance

Used for policy, standard, or SLA alignment.

Subcategories

SLA Compliance

Policy Compliance

Regulation Alignment

Audit Readiness

Exception Rate

Non-Compliance Count

High-Risk Flag

Compliance Score

Examples

SLA Uptime Compliance (%)

Non-Compliant Downtime Events

Critical Compliance Breach Indicator

1. Field-Level & Selection Metrics (Most Basic)
These are derived directly from user inputs.

Selection Frequency/Count: Simple count/percentage of users who chose each option (for radio, dropdown, checkbox).

Report: "70% selected 'Satisfied' on Q1."

Aggregated Scores: Summation of assigned score values from selected options across the form or per section.

Report: "Average total risk score: 58/100."

Text & Number Field Analytics:

Averages/Means: For number fields (e.g., average age, average satisfaction rating 1-10).

Ranges & Distributions: Min, max, standard deviation (e.g., "Scores ranged from 12 to 95").

Text Analysis: Word frequency, sentiment analysis (positive/negative keywords), topic extraction (for longer text fields).

Completion Rate: Percentage of users who filled vs. skipped a specific text/number field.
2. Section-Level & Cross-Field Metrics
These analyze relationships within a section or across the form.

Section Completion Rate: % of users who completed all mandatory (or any) fields in a section.

Intra-Section Correlation: Does an answer in one field predict another in the same section? (e.g., Users who selected "IT Department" in Field A frequently select "High" for "Software Needs" in Field B).

Conditional Scoring: Dynamic scores where the value of one selection modifies the weight of another. (e.g., Selecting "Has allergy" gives bonus points to severity-related follow-up questions).

Composite Indexes: Create a single metric from multiple fields in a section (e.g., a "Wellness Score" from 5 different health-related questions, each with its own sub-score).

3. Form-Wide & Aggregate Metrics
These look at the form as a whole.

Overall Form Completion Rate & Abandonment Points: Identify at which section/field users most frequently drop off.

Total Time to Complete: Average, median, and distribution of time spent on the entire form.

Overall Score Distribution: Histogram of total scores across all respondents.

Segmentation by Total Score: Bucket users into categories (e.g., Low/Medium/High Risk) based on total score thresholds.

4. Temporal & Trend Metrics
Analyze how data changes over time.

Score Trends: Track average total score or section score week-over-week, month-over-month.

Answer Shift: Monitor how selection frequencies for key options change over time.

Submission Volume: Number of forms submitted per time period.

5. User/Cohort-Based Metrics
Segment reports by user properties (if available) or by their form responses.

Demographic/Profile Segmentation: Compare metrics for different user groups (e.g., by department, location, tenure) if this data is captured in the form or linked from a user profile.

Behavioral Cohorts: Create cohorts based on a key answer (e.g., "All users who rated service < 5") and analyze their other responses.

Score by Segment: Compare average scores across different user segments.

6. Advanced Analytical Metrics
These require more complex data processing.

Weighted Scoring Models: Beyond simple sums, use weighted averages where some sections/fields contribute more to the final score.

Predictive Indicators: Use statistical models to identify which 2-3 form answers are most predictive of a high total score or a specific outcome.

Gap Analysis: Compare scores between different sections to identify disparities (e.g., "High knowledge score but low compliance score").

Benchmarking: Compare an individual's or group's scores against a historical baseline or an industry standard.

Reporting Framework & Implementation
To operationalize these, structure your reporting system around these layers:

Raw Data Layer: Stores every form submission with timestamps, user ID (if any), and all question/answer pairs with their base scores.

Calculated Metrics Layer: Pre-compute or dynamically calculate:

Per Submission: Total score, section scores, completion status.

Aggregate: Averages, frequencies, trends.

Report Types:

Executive Dashboards: High-level KPIs - Completion rate, average total score, trend lines.

Section Performance Reports: Drill down into each section with frequency analysis and average section scores.

Score Distribution Reports: Histograms, percentile rankings.

Cohort Comparison Reports: Side-by-side metrics for different user groups.

Drill-Through Reports: Start from an aggregate metric (e.g., "Low average score in Section 3") and drill down to see the individual submissions and open-text comments that contributed to it.

Time-Series Reports: Charts showing how key metrics evolve.