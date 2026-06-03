// @ts-check
// Docusaurus site config for the Nodely documentation.
const {themes} = require('prism-react-renderer');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Nodely',
  tagline: 'A native Avalonia diagram & node-editor toolkit — no SVG, no JS, no WebView',

  url: 'https://araxis.github.io',
  baseUrl: '/nodely/',
  organizationName: 'araxis',
  projectName: 'nodely',
  trailingSlash: false,

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Render ```mermaid code blocks as live diagrams.
  markdown: {
    mermaid: true,
  },
  themes: ['@docusaurus/theme-mermaid'],

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: '/', // docs are the whole site (no separate landing page)
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/araxis/nodely/edit/main/website/',
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      colorMode: {
        defaultMode: 'dark',
        respectPrefersColorScheme: true,
      },
      mermaid: {
        theme: {light: 'neutral', dark: 'dark'},
      },
      navbar: {
        title: 'Nodely',
        items: [
          {type: 'docSidebar', sidebarId: 'docs', position: 'left', label: 'Docs'},
          {href: 'https://github.com/araxis/nodely', label: 'GitHub', position: 'right'},
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {label: 'Introduction', to: '/'},
              {label: 'Getting started', to: '/getting-started'},
            ],
          },
          {
            title: 'More',
            items: [{label: 'GitHub', href: 'https://github.com/araxis/nodely'}],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Nodely contributors. Built with Docusaurus.`,
      },
      prism: {
        theme: themes.github,
        darkTheme: themes.dracula,
        additionalLanguages: ['csharp', 'bash', 'json'],
      },
    }),
};

module.exports = config;
