# Sprint Burndown Visualization

This guide covers how to generate, configure, and interpret sprint burndown charts using Jira Analytics CLI.

## Overview

A burndown chart shows the remaining story points (or issue count) over the course of a sprint. The CLI renders charts as **PNG**, **SVG**, or **JSON** using SkiaSharp and can save them to any directory.

---

## Quick Start

```bash
# Export a burndown chart for sprint 42 as a PNG
jira-analytics-cli burndown --project PROJ --sprint-id 42 --output burndown.png

# Export as SVG for embedding in web pages
jira-analytics-cli burndown --project PROJ --sprint-id 42 --output burndown.svg --format svg

# Export raw data as JSON for custom processing
jira-analytics-cli burndown --project PROJ --sprint-id 42 --output burndown.json --format json

# Save to a specific directory (creates it if it does not exist)
jira-analytics-cli burndown --project PROJ --sprint-id 42 --output burndown.png \
  --output-dir ./reports/2026-05/
```

---

## Output Formats

| Format | Flag | Use Case |
|--------|------|---------|
| PNG | `--format png` (default) | Sharing in Slack, email, Confluence |
| SVG | `--format svg` | Embedding in HTML/Markdown docs |
| JSON | `--format json` | Building custom dashboards or BI tools |

---

## Understanding the Chart

The generated chart includes two lines:

- **Grey dashed line** — ideal burndown (linear progress from total points on day 1 to zero on the last day)
- **Red solid line** — actual remaining story points per day

A sprint is on track when the red line stays at or below the grey dashed line.

```
Story Points
│
50 │●
   │ ╲  (ideal)
   │  ╲ - - - - - - -
   │   ●                ← actual behind ideal
   │    ╲              ●
   │     ╲         ●
   │      ● - - ●
   │              ●
 0 └────────────────────── Days
   Day 1              Day 14
```

---

## Configuration

### appsettings.json

```json
{
  "Jira": {
    "BaseUrl": "https://your-workspace.atlassian.net",
    "ApiToken": "your-api-token"
  },
  "Burndown": {
    "ChartWidth": 1200,
    "ChartHeight": 800,
    "ShowIdealLine": true,
    "DefaultFormat": "png"
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `JIRA_BASE_URL` | Jira instance URL | (required) |
| `JIRA_API_TOKEN` | Jira API token | (required) |

---

## Examples

### PNG in a CI/CD Pipeline

```yaml
# .github/workflows/sprint-report.yml
- name: Generate burndown chart
  run: |
    jira-analytics-cli burndown \
      --project ${{ vars.JIRA_PROJECT }} \
      --sprint-id ${{ vars.SPRINT_ID }} \
      --output burndown.png \
      --output-dir ./artifacts/

- name: Upload chart artifact
  uses: actions/upload-artifact@v4
  with:
    name: burndown-chart
    path: ./artifacts/burndown.png
```

### SVG Embedded in a Markdown Report

```bash
jira-analytics-cli burndown --project PROJ --sprint-id 42 \
  --output sprint-42-burndown.svg --format svg

# Then reference it in your Markdown
echo "![Sprint 42 Burndown](sprint-42-burndown.svg)" >> sprint-report.md
```

### Batch Export for Multiple Sprints

```bash
#!/usr/bin/env bash
# generate burndown charts for sprints 40-45

PROJECT="PROJ"
OUTPUT_DIR="./reports/burndown"

for SPRINT_ID in 40 41 42 43 44 45; do
  jira-analytics-cli burndown \
    --project "$PROJECT" \
    --sprint-id "$SPRINT_ID" \
    --output "sprint-${SPRINT_ID}.png" \
    --output-dir "$OUTPUT_DIR"
  echo "Generated $OUTPUT_DIR/sprint-${SPRINT_ID}.png"
done
```

### JSON Data for Custom Dashboards

```bash
jira-analytics-cli burndown --project PROJ --sprint-id 42 \
  --output data.json --format json

# Example output structure
cat data.json
```

```json
[
  {
    "timestamp": "2026-05-01T09:00:00Z",
    "sprintId": 42,
    "totalStoryPoints": 50,
    "completedStoryPoints": 0,
    "remainingStoryPoints": 50,
    "totalIssueCount": 12,
    "completedIssueCount": 0,
    "remainingIssueCount": 12
  },
  {
    "timestamp": "2026-05-02T09:00:00Z",
    "sprintId": 42,
    "totalStoryPoints": 50,
    "completedStoryPoints": 8,
    "remainingStoryPoints": 42,
    "totalIssueCount": 12,
    "completedIssueCount": 2,
    "remainingIssueCount": 10
  }
]
```

---

## Interpreting Results

### Sprint is on track

The red line closely follows or stays below the ideal grey dashed line. The team is completing story points at the planned rate.

### Sprint is behind

The red line is above the ideal line. Story points are being completed slower than planned. Consider:
- Re-prioritising unfinished items
- Moving lower-priority issues to the backlog
- Discussing blockers in the next stand-up

### Flat section in the red line

No story points were completed on those days. Common causes:
- Weekend / public holiday (use `--exclude-weekends` in developer reports)
- Team blocked on external dependency
- Issues awaiting review / QA sign-off

### Red line drops sharply at the end

A large batch of issues was resolved at the end of the sprint. This is a common anti-pattern — aim for a smoother daily burndown by completing smaller, incremental pieces of work.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Chart is blank / only axes visible | Sprint has no issues or no story points | Verify sprint ID and that issues have story-point estimates in Jira |
| `Sprint N not found` error | Invalid sprint ID | Run `jira-analytics-cli analytics --project PROJ` to list available sprint IDs |
| Output file not created | Directory does not exist | Pass `--output-dir <path>` — the CLI creates it automatically |
| Ideal line is a flat horizontal | Sprint has no start or end date set | Set the sprint's start/end dates in Jira |
| PNG looks pixelated on retina displays | Fixed 1200×800 resolution | Export as SVG for resolution-independent output |
| `TaskCanceledException` in logs | API request timed out | Check network connectivity; the default timeout is 30 seconds |

---

## Related Commands

```bash
# See overall velocity across multiple sprints (includes per-sprint burndown summary)
jira-analytics-cli analytics --project PROJ --sprints 5

# Export velocity chart as image
jira-analytics-cli export --project PROJ --format png --output velocity.png

# Check Jira API connectivity
jira-analytics-cli health
```

---

## See Also

- [Getting Started](getting-started.md)
- [API Reference](api-reference.md)
- [Custom Metrics Plugin Guide](custom-metrics-plugin-guide.md)
