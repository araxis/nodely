# Nodely documentation site

The Nodely docs are built as a static site and published to GitHub Pages by `.github/workflows/docs.yml`.

## Local development

```bash
cd website
npm install
npm start        # dev server with hot reload
npm run build    # production build into ./build
npm run serve    # serve the production build locally
```

## Publishing

Pushing to `main` with changes under `website/` runs the **Docs** workflow: it builds the site and deploys it
to GitHub Pages. One-time repo setup: **Settings → Pages → Build and deployment → Source: "GitHub Actions"**.
