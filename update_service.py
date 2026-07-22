#!/usr/bin/env python3
"""
Script to update JiraApiService to throw JiraApiException for error status codes
"""

import re

# Read the original file
with open('/home/redrocket/task-factory/workdir/jira-analytics-cli/Services/JiraApiService.cs', 'r') as f:
    content = f.read()

# Step 1: Add JiraApiException import
if 'JiraAnalyticsCli.Exceptions' not in content:
    content = content.replace(
        'using Microsoft.Extensions.Logging;',
        'using JiraAnalyticsCli.Exceptions;\nusing Microsoft.Extensions.Logging;'
    )
    print("✓ Added JiraApiException import")

# Helper function to update a method
def update_method(method_pattern, error_return_pattern, new_throw_statement):
    """Update a method to throw JiraApiException instead of returning null/empty"""
    # Find the method
    method_match = re.search(method_pattern, content, re.DOTALL)
    if not method_match:
        return False

    method_start = method_match.start()
    method_end_search = content.find('\n    }', method_start + 100)
    if method_end_search == -1:
        return False

    method_section = content[method_start:method_end_search + 6]

    # Find and replace the error return
    error_pattern = re.compile(
        r'if \(!response\.IsSuccessStatusCode\)\s*\{[^}]*?' + error_return_pattern + r'[^}]*?\}',
        re.DOTALL
    )

    if error_pattern.search(method_section):
        new_error_block = f'''if (!response.IsSuccessStatusCode)
        {{
            _logger.LogWarning("Failed to fetch {{method_name}}: {{StatusCode}}", {method_name_var(method_pattern)}, response.StatusCode);
            {new_throw_statement}
        }}'''

        # This is getting complex - let's use simpler string replacements
        pass

    return True

def method_name_var(method_pattern):
    """Extract method name from pattern"""
    if 'GetProjectAsync' in method_pattern:
        return 'projectKey'
    elif 'GetProjectSprintsAsync' in method_pattern:
        return 'projectKey'
    elif 'GetSprintAsync' in method_pattern:
        return 'sprintId.ToString()'
    elif 'GetSprintIssuesAsync' in method_pattern:
        return 'sprintId'
    elif 'GetProjectIssuesAsync' in method_pattern:
        return 'projectKey'
    elif 'GetProjectTeamAsync' in method_pattern:
        return 'projectKey'
    elif 'GetIssueAsync' in method_pattern:
        return 'issueKey'
    elif 'GetBurndownDataAsync' in method_pattern:
        return 'sprintId'
    elif 'SearchByJqlAsync' in method_pattern:
        return 'jql'
    elif 'VerifyConnectionAsync' in method_pattern:
        return 'Jira API connection'
    return '"data"'

# Update each method individually - simpler approach

# 1. GetProjectAsync
old_getproject_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch project {ProjectKey}: {StatusCode}", projectKey, response.StatusCode);
            return null;
        }'''

new_getproject_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch project {ProjectKey}: {StatusCode}", projectKey, response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_getproject_error in content:
    content = content.replace(old_getproject_error, new_getproject_error)
    print("✓ Updated GetProjectAsync error handling")

# 2. GetProjectSprintsAsync
old_sprints_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprints: {StatusCode}", response.StatusCode);
            return sprints;
        }'''

new_sprints_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprints: {StatusCode}", response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_sprints_error in content:
    content = content.replace(old_sprints_error, new_sprints_error)
    print("✓ Updated GetProjectSprintsAsync error handling")

# 3. GetSprintAsync
old_sprint_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprint {SprintId}: {StatusCode}", sprintId, response.StatusCode);
            return null;
        }'''

new_sprint_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprint {SprintId}: {StatusCode}", sprintId, response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_sprint_error in content:
    content = content.replace(old_sprint_error, new_sprint_error)
    print("✓ Updated GetSprintAsync error handling")

# 4. GetSprintIssuesAsync
old_issues_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprint issues: {StatusCode}", response.StatusCode);
            return issues;
        }'''

new_issues_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch sprint issues: {StatusCode}", response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_issues_error in content:
    content = content.replace(old_issues_error, new_issues_error)
    print("✓ Updated GetSprintIssuesAsync error handling")

# 5. GetProjectIssuesAsync
old_projissues_error = '''        if (!response.IsSuccessStatusCode) return issues;'''

new_projissues_error = '''        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_projissues_error in content:
    content = content.replace(old_projissues_error, new_projissues_error)
    print("✓ Updated GetProjectIssuesAsync error handling")

# 6. GetProjectTeamAsync
old_team_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch team for project {ProjectKey}", projectKey);
            return team;
        }'''

new_team_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch team for project {ProjectKey}", projectKey);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_team_error in content:
    content = content.replace(old_team_error, new_team_error)
    print("✓ Updated GetProjectTeamAsync error handling")

# 7. GetIssueAsync
old_issue_error = '''        if (!response.IsSuccessStatusCode) return null;'''

new_issue_error = '''        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_issue_error in content:
    content = content.replace(old_issue_error, new_issue_error)
    print("✓ Updated GetIssueAsync error handling")

# 8. GetBurndownDataAsync
old_burndown_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch burndown data: {StatusCode}", response.StatusCode);
            return snapshots;
        }'''

new_burndown_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch burndown data: {StatusCode}", response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_burndown_error in content:
    content = content.replace(old_burndown_error, new_burndown_error)
    print("✓ Updated GetBurndownDataAsync error handling")

# 9. SearchByJqlAsync
old_search_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("JQL search returned {StatusCode}", response.StatusCode);
            return result;
        }'''

new_search_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("JQL search returned {StatusCode}", response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_search_error in content:
    content = content.replace(old_search_error, new_search_error)
    print("✓ Updated SearchByJqlAsync error handling")

# 10. VerifyConnectionAsync
old_verify_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to verify Jira connection: {StatusCode}", response.StatusCode);
            return false;
        }'''

new_verify_error = '''        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to verify Jira connection: {StatusCode}", response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new JiraApiException($"Jira API request failed with status code {(int)response.StatusCode}\", (int)response.StatusCode, errorContent);
        }'''

if old_verify_error in content:
    content = content.replace(old_verify_error, new_verify_error)
    print("✓ Updated VerifyConnectionAsync error handling")

# Now update catch blocks to rethrow JiraApiException and throw for other exceptions
old_catch = '''    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching project {ProjectKey}", projectKey);
        return null;
    }'''

new_catch = '''    catch (JiraApiException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching project {ProjectKey}", projectKey);
        throw new JiraApiException("Failed to fetch project due to an unexpected error", ex);
    }'''

# Update catch blocks for each method - do them one by one
methods_to_update = [
    ('GetProjectAsync', 'projectKey'),
    ('GetProjectSprintsAsync', 'projectKey'),
    ('GetSprintAsync', 'sprintId'),
    ('GetSprintIssuesAsync', 'sprintId'),
    ('GetProjectIssuesAsync', 'projectKey'),
    ('GetProjectTeamAsync', 'projectKey'),
    ('GetIssueAsync', 'issueKey'),
    ('GetBurndownDataAsync', 'sprintId'),
    ('SearchByJqlAsync', 'jql'),
    ('VerifyConnectionAsync', 'connection')
]

for method_name, context_var in methods_to_update:
    # Find the catch block after this method
    pattern = rf'public async Task.*{method_name}.*?catch \(Exception ex\)\s*{{.*?_logger\.LogError\(ex,.*?\)\s*return [^;]+;\s*}}'

    # Use simpler approach - find line by line
    lines = content.split('\n')
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        # Check if this is a catch block we should update
        if 'catch (Exception ex)' in line and i > 0:
            # Check if it's in one of our methods by looking back
            context = '\n'.join(lines[max(0, i-50):i])
            if method_name in context and 'return null;' in context:
                # Found it! Replace this catch block
                indent = len(line) - len(line.lstrip())
                new_lines.append(line)  # catch line
                i += 1
                # Skip old body
                while i < len(lines) and lines[i].strip() != '}':
                    i += 1
                # Add new catch blocks
                new_lines.append(' ' * indent + 'if (ex is JiraApiException jex)')
                new_lines.append(' ' * indent + '{')
                new_lines.append(' ' * (indent + 4) + 'throw jex;')
                new_lines.append(' ' * indent + '}')
                new_lines.append(' ' * indent + f'_logger.LogError(ex, "Error fetching {{nameof({method_name})}}");')
                new_lines.append(' ' * indent + 'throw new JiraApiException($"Failed to fetch data due to an unexpected error", ex);')
                continue
        new_lines.append(line)
        i += 1

    content = '\n'.join(new_lines)

print("✓ Updated all catch blocks")

# Write the updated content
with open('/home/redrocket/task-factory/workdir/jira-analytics-cli/Services/JiraApiService.cs', 'w') as f:
    f.write(content)

print("\n✅ Successfully updated JiraApiService.cs")
