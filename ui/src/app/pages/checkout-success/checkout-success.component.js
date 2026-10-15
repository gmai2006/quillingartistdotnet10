(function () {
  'use strict';

  // Stripe redirects here with ?session_id=...; the backend verifies the
  // payment and records the order.
  angular.module('quillingApp')
    .component('checkoutSuccessPage', {
      templateUrl: 'app/pages/checkout-success/checkout-success.template.html',
      controller: ['$location', 'CartService', 'CheckoutService', function ($location, CartService, CheckoutService) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.status = 'loading'; // loading | success | error
          $ctrl.order = null;
          $ctrl.error = '';

          const sessionId = $location.search().session_id;
          if (!sessionId) {
            $ctrl.status = 'error';
            $ctrl.error = 'Missing checkout session.';
            return;
          }

          CheckoutService.confirm(sessionId)
            .then(order => {
              $ctrl.order = order;
              $ctrl.status = 'success';
              CartService.clear();
            })
            .catch(err => {
              $ctrl.status = 'error';
              $ctrl.error = err.message;
            });
        };
      }],
    });
})();
