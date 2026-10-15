(function () {
  'use strict';

  angular.module('quillingApp')
    .factory('ArrayUtils', [function () {
      return {
        // Fisher-Yates shuffle, in place; returns the same array.
        shuffle(array) {
          for (let i = array.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [array[i], array[j]] = [array[j], array[i]];
          }
          return array;
        },
      };
    }]);
})();
