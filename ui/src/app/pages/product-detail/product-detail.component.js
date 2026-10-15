(function () {
  'use strict';

  angular.module('quillingApp')
    .component('productDetailPage', {
      bindings: {
        products: '<',
      },
      templateUrl: 'app/pages/product-detail/product-detail.template.html',
      controller: ['$routeParams', '$location', 'ProductService', 'CartService', 'ArrayUtils',
        function ($routeParams, $location, ProductService, CartService, ArrayUtils) {
          const $ctrl = this;

          $ctrl.$onInit = function () {
            $ctrl.activeTab = 'details';
            $ctrl.quantity = 1;
          };

          $ctrl.$onChanges = function (changes) {
            if (!changes.products) return;
            const products = $ctrl.products || [];
            $ctrl.product = ProductService.findById(products, $routeParams.id);
            // Shuffled once per page view (React re-shuffled on every render).
            $ctrl.featured = ArrayUtils.shuffle(products.slice(0, 6));
          };

          $ctrl.decrement = function () {
            $ctrl.quantity = Math.max(1, $ctrl.quantity - 1);
          };

          $ctrl.increment = function () {
            $ctrl.quantity += 1;
          };

          $ctrl.addToCart = function () {
            CartService.add($ctrl.product, $ctrl.quantity);
          };

          $ctrl.buyNow = function () {
            $ctrl.addToCart();
            $location.path('/checkout');
          };
        }],
    });
})();
