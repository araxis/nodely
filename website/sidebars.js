// @ts-check

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  docs: [
    'intro',
    'getting-started',
    {
      type: 'category',
      label: 'Guides',
      collapsed: false,
      items: [
        'guides/custom-nodes',
        'guides/links',
        'guides/selection-and-clipboard',
        'guides/undo-redo',
        'guides/serialization',
        'guides/layout',
        'guides/theming',
        'guides/extensibility',
      ],
    },
    'architecture',
  ],
};

module.exports = sidebars;
