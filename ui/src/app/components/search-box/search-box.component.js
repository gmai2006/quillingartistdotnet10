(function () {
  'use strict';

  // Product name search. Navigates to /searchResult, or shows a short-lived
  // "empty result" popover when nothing matches.
  angular.module('quillingApp')
    .component('searchBox', {
      templateUrl: 'app/components/search-box/search-box.template.html',
      controller: ['$location', '$timeout', 'ProductService', function ($location, $timeout, ProductService) {
        const $ctrl = this;
        let hideTimer = null;

        $ctrl.$onInit = function () {
          $ctrl.searchVal = '';
          $ctrl.showEmpty = false;
        };

        $ctrl.search = function () {
          if (!$ctrl.searchVal) return;

          ProductService.getAll().then(products => {
            const result = ProductService.search(products, $ctrl.searchVal);
            if (result.length < 1) {
              showEmptyFor(2);
            } else {
              ProductService.setSearchResults(result);
              $location.path('/searchResult');
            }
          });
        };

        function showEmptyFor(seconds) {
          $ctrl.showEmpty = true;
          $timeout.cancel(hideTimer);
          hideTimer = $timeout(() => { $ctrl.showEmpty = false; }, seconds * 1000);
        }

        $ctrl.$onDestroy = function () {
          $timeout.cancel(hideTimer);
        };
      }],
    });
})();
