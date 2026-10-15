(function () {
  'use strict';

  angular.module('quillingApp')
    .component('cardFeaturedProduct', {
      bindings: {
        products: '<',
      },
      templateUrl: 'app/components/card-featured-product/card-featured-product.template.html',
      controller: [function () {
        const $ctrl = this;

        // One entry per star; falls back to 5 when the product has no rating.
        $ctrl.stars = function (product) {
          return new Array(product.star || 5);
        };
      }],
    });
})();
