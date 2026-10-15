(function () {
  'use strict';

  angular.module('quillingApp')
    .factory('ProductService', ['$http', '$log', 'API_CONFIG', function ($http, $log, API_CONFIG) {
      let productsPromise = null;
      let searchResults = [];

      // Loads the whole catalog once and shares the promise with every caller.
      // A failed load resolves to [] (the pages show their empty state) and is
      // retried on the next call.
      function getAll() {
        if (!productsPromise) {
          productsPromise = $http.get(API_CONFIG.productsUrl, {
            params: { page: 0, pageSize: 200 },
          })
            .then(res => (res.data && res.data.content) || [])
            .catch(err => {
              $log.error('[API ERROR] loading products', err.status, err.data);
              productsPromise = null;
              return [];
            });
        }
        return productsPromise;
      }

      function findById(products, id) {
        return products.find(item => String(item.id) === String(id));
      }

      function search(products, term) {
        const needle = term.toLowerCase();
        return products.filter(product => product.name.toLowerCase().includes(needle));
      }

      return {
        getAll,
        findById,
        search,
        getSearchResults: () => searchResults,
        setSearchResults: results => { searchResults = results; },
      };
    }]);
})();
