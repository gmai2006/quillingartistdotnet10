(function () {
  'use strict';

  angular.module('quillingApp')
    .component('searchResultPage', {
      templateUrl: 'app/pages/search-result/search-result.template.html',
      controller: ['ProductService', function (ProductService) {
        const $ctrl = this;

        // A getter (not a copy) so a new search from the header, while already
        // on /searchResult, re-renders without a route change.
        $ctrl.results = ProductService.getSearchResults;
      }],
    });
})();
