(function () {
  'use strict';

  // In-memory shopping cart (port of models/Shoppingcart.js). Entries are
  // keyed by product id: { product, count }.
  angular.module('quillingApp')
    .service('CartService', [function () {
      const entries = new Map();

      this.add = function (product, quantity) {
        if (!product) return;
        const qty = quantity || 1;
        if (entries.has(product.id)) {
          entries.get(product.id).count += qty;
        } else {
          entries.set(product.id, { product: product, count: qty });
        }
      };

      // Decrements one unit; removes the entry when it reaches zero.
      this.remove = function (id) {
        if (!entries.has(id)) return;
        const entry = entries.get(id);
        if (entry.count > 1) entry.count -= 1;
        else entries.delete(id);
      };

      this.removeAll = function (id) {
        entries.delete(id);
      };

      this.clear = function () {
        entries.clear();
      };

      this.items = function () {
        return Array.from(entries.values());
      };

      this.isEmpty = function () {
        return entries.size === 0;
      };

      this.count = function () {
        let total = 0;
        entries.forEach(entry => { total += entry.count; });
        return total;
      };

      this.totalCost = function () {
        let total = 0;
        entries.forEach(entry => { total += entry.product.price * entry.count; });
        return total;
      };
    }]);
})();
