import { withMermaid } from 'vitepress-plugin-mermaid'

export default withMermaid({
  lang: 'en-US',
  title: 'Nodely',
  description: 'A native Avalonia diagram and node-editor toolkit.',
  base: '/nodely/',
  cleanUrls: true,
  outDir: '../build',
  lastUpdated: true,
  themeConfig: {
    nav: [
      { text: 'Docs', link: '/' },
      { text: 'Packages', link: 'https://www.nuget.org/packages/Nodely.Avalonia' },
      { text: 'GitHub', link: 'https://github.com/araxis/nodely' },
    ],
    sidebar: [
      { text: 'Introduction', link: '/' },
      { text: 'Getting started', link: '/getting-started' },
      {
        text: 'Guides',
        collapsed: false,
        items: [
          { text: 'Custom nodes', link: '/guides/custom-nodes' },
          { text: 'Links, markers, labels & bend points', link: '/guides/links' },
          { text: 'Selection, clipboard & context menu', link: '/guides/selection-and-clipboard' },
          { text: 'Undo / redo', link: '/guides/undo-redo' },
          { text: 'Save & load', link: '/guides/serialization' },
          { text: 'Auto-layout & graph queries', link: '/guides/layout' },
          { text: 'Theming', link: '/guides/theming' },
          { text: 'Extensibility', link: '/guides/extensibility' },
          { text: 'API pack', link: '/guides/api' },
          { text: 'Database pack', link: '/guides/database' },
          { text: 'MindMap pack', link: '/guides/mindmap' },
          { text: 'StateMachine pack', link: '/guides/statemachine' },
          { text: 'UML pack', link: '/guides/uml' },
          { text: 'Workflow pack', link: '/guides/workflow' },
          { text: 'Recipes', link: '/guides/recipes' },
          { text: 'Release checklist', link: '/guides/release-checklist' },
        ],
      },
      { text: 'Architecture', link: '/architecture' },
    ],
    socialLinks: [
      { icon: 'github', link: 'https://github.com/araxis/nodely' },
    ],
    search: {
      provider: 'local',
    },
    outline: {
      level: [2, 3],
    },
    editLink: {
      pattern: 'https://github.com/araxis/nodely/edit/main/website/docs/:path',
      text: 'Edit this page',
    },
    footer: {
      message: 'Released under the MIT License.',
      copyright: `Copyright © 2026-present Nodely contributors`,
    },
  },
})
