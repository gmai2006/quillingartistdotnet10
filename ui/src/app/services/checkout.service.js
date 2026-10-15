(function () {
  'use strict';

  angular.module('quillingApp')
    .factory('CheckoutService', ['$http', '$q', function ($http, $q) {
      // Normalises an $http failure into an Error carrying the API's message.
      function toError(fallback) {
        return function (res) {
          const message = (res && res.data && res.data.error) || fallback;
          return $q.reject(new Error(message));
        };
      }

      return {
        // Returns the Stripe Checkout Session ({ url, ... }) for the cart items.
        createSession(items, successUrl, cancelUrl) {
          return $http.post('/api/checkout/create-session', {
            items: items.map(entry => ({ productId: entry.product.id, quantity: entry.count })),
            successUrl: successUrl,
            cancelUrl: cancelUrl,
          })
            .then(res => res.data)
            .catch(toError('Unable to start checkout.'));
        },

        // Verifies the paid session; resolves to { orderId, customerName, total, ... }.
        confirm(sessionId) {
          return $http.post('/api/checkout/confirm', { sessionId: sessionId })
            .then(res => res.data)
            .catch(toError('Unable to confirm payment.'));
        },
      };
    }]);
})();
