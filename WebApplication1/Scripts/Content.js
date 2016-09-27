(function () {
    var app = angular.module('ContentApp', ['ngRoute']);
    app.controller('HomeController', function ($scope) {
        $scope.init = function (contents) {
            $scope.contents = contents;        
    }});
})();