(function () {
  'use strict';

  /**
   * Renders a Lucide icon as an inline <svg> (replacement for lucide-react).
   * This is pure DOM work, so it is a directive rather than a component.
   *
   *   <lucide-icon name="ShoppingCart" icon-class="h-6 w-6"></lucide-icon>
   *   <lucide-icon name="{{ $ctrl.isOpen ? 'X' : 'Menu' }}" size="18"></lucide-icon>
   *
   * name        PascalCase icon name, as imported from lucide-react
   * icon-class  classes applied to the <svg> (the host is display: contents)
   * size        width/height in px (default 24)
   * stroke-width
   */
  angular.module('quillingApp')
    .directive('lucideIcon', ['$window', '$log', function ($window, $log) {
      return {
        restrict: 'E',
        link: function (scope, element, attrs) {
          let rendered = null;

          function render() {
            const key = `${attrs.name}|${attrs.iconClass}`;
            if (key === rendered) return;
            rendered = key;

            const lucide = $window.lucide;
            const iconNode = lucide && lucide.icons[attrs.name];
            element.empty();
            if (!iconNode) {
              $log.warn(`lucide-icon: unknown icon "${attrs.name}"`);
              return;
            }

            const size = attrs.size || 24;
            const svgAttrs = {
              width: size,
              height: size,
              'aria-hidden': 'true',
              class: `lucide ${attrs.iconClass || ''}`.trim(),
            };
            if (attrs.strokeWidth) svgAttrs['stroke-width'] = attrs.strokeWidth;

            element.append(lucide.createElement(iconNode, svgAttrs));
          }

          // $observe fires once after interpolation, and again whenever an
          // interpolated attribute (e.g. a toggled icon name) changes.
          attrs.$observe('name', render);
          attrs.$observe('iconClass', render);
        },
      };
    }]);
})();
