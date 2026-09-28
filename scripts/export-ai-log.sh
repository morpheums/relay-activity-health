#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "$0")/.." && pwd)"
projects_dir="${CLAUDE_CONFIG_DIR:-$HOME/.claude}/projects"
raw_dir="$repo_root/ai-log/raw"
sessions_dir="$repo_root/ai-log/sessions"
redact_file="$repo_root/.ai-log-redact"
[ -s "$redact_file" ] || { echo "Missing $redact_file (one literal secret per line, git-ignored)" >&2; exit 1; }

mkdir -p "$raw_dir" "$sessions_dir"

render_markdown() {
  jq -r --arg max 1500 '
    def clip: if (tostring | length) > ($max | tonumber) then (tostring | .[0:($max | tonumber)]) + " …[clipped in markdown; full text in raw/]" else tostring end;
    select(.type == "user" or .type == "assistant")
    | .timestamp as $at
    | if .type == "user" and (.message.content | type) == "string" then "\n### 🧑 USER — \($at)\n\n\(.message.content)\n"
      elif .type == "user" then (.message.content[]? | select(.type == "tool_result") | "\n<details><summary>tool result</summary>\n\n```\n\(.content | clip)\n```\n</details>\n")
      else (.message.content[]? |
        if .type == "text" then "\n### 🤖 ASSISTANT — \($at)\n\n\(.text)\n"
        elif .type == "tool_use" then "\n**tool call — \(.name)**\n```json\n\(.input | tojson | clip)\n```\n"
        else empty end)
      end' "$1"
}

for project in "$projects_dir"/*Qualitara*; do
  while IFS= read -r transcript; do
    relative="${transcript#"$project"/}"
    target="$raw_dir/$relative"
    mkdir -p "$(dirname "$target")"
    REDACT_FILE="$redact_file" perl -pe '
      BEGIN { open my $fh, "<", $ENV{REDACT_FILE} or die; chomp(@secrets = grep { /\S/ } <$fh>); }
      for my $secret (@secrets) { s/\Q$secret\E/<redacted>/g }
      s/(SA_PASSWORD[\\"=: ]+)[^\\"\s,]+/$1<redacted>/g;
    ' "$transcript" > "$target"
    rendered="$sessions_dir/${relative%.jsonl}.md"
    mkdir -p "$(dirname "$rendered")"
    render_markdown "$target" > "$rendered"
  done < <(find "$project" -name '*.jsonl' -not -path '*/memory/*')
done

echo "Exported $(find "$raw_dir" -name '*.jsonl' | wc -l | tr -d ' ') transcripts to ai-log/raw and ai-log/sessions"
