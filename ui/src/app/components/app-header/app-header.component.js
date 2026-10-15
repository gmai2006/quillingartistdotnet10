(function () {
  'use strict';

  angular.module('quillingApp')
    .component('appHeader', {
      templateUrl: 'app/components/app-header/app-header.template.html',
      controller: ['CartService', function (CartService) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.cart = CartService;
        };
      }],
    });
})();
