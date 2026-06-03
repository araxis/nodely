# Nodely documentation site

The Nodely docs, built with [Docusaurus](https://docusaurus.io/) and published to GitHub Pages by
`.github/workflows/docs.yml`.

## Local development

```bash
cd website
npm install
npm start        # dev server with hot reload at http://localhost:3000
npm run build    # production build into ./build
npm run serve    # serve the production build locally
```

## Publishing

Pushing to `main` with changes under `website/` runs the **Docs** workflow: it builds the site and deploys it
to GitHub Pages. One-time repo setup: **Settings → Pages → Build and deployment → Source: "GitHub Actions"**.

> The `url`, `baseUrl`, `organizationName`, and `projectName` in `docusaurus.config.js` are placeholders for a
> project site at `https://nodely.github.io/nodely/`. Update them to your real GitHub org/repo (and the
> `editUrl` / GitHub links) before publishing.
