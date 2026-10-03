# AI PR Reviewer (GitHub Actions) Setup

This repo uses a GitHub Actions workflow to post an **AI-generated code review comment** on every Pull Request.

It uses **NVIDIA's OpenAI-compatible endpoint**.

## 1) Add the required secret

1. Go to **GitHub → Repo → Settings → Secrets and variables → Actions**
2. Under **Repository secrets**, click **New repository secret**
3. Create:

- Name: `NVIDIA_API_KEY`
- Value: your NVIDIA API key

## 2) Configure the model

Recommended model (NVIDIA hosted):
- `openai/gpt-oss-20b`

To set it:

1. Go to **Settings → Secrets and variables → Actions → Variables**
2. Add a repository variable:

- Name: `NVIDIA_MODEL`
- Value: `openai/gpt-oss-20b`

> The workflow defaults to `openai/gpt-oss-20b` if you don't set `NVIDIA_MODEL`.

## 3) (Optional) Configure endpoint + limits

Repository variables (optional):

- `NVIDIA_BASE_URL` (default: `https://integrate.api.nvidia.com/v1`)
- `AI_REVIEW_MAX_DIFF_CHARS` (default: `120000`) – prevents huge prompts

## 4) (Optional) Limit what files are reviewed

Repository variables (comma-separated):

- `AI_REVIEW_INCLUDE_EXTENSIONS` e.g. `.ts,.js,.py,.java,.cs`
- `AI_REVIEW_EXCLUDE_PATHS` e.g. `dist/,build/,node_modules/,.github/`

## 5) Test

1. Commit/push the workflow changes to the default branch
2. Open a PR (or push new commits to an existing PR)
3. In the PR conversation, you should see a comment titled **AI Code Review**.

## Notes / Troubleshooting

- If the PR is from a fork, GitHub may not provide secrets to workflows by default.
- Very large diffs may be truncated.
- The workflow updates its own previous comment (does not spam comments) using a hidden marker.
