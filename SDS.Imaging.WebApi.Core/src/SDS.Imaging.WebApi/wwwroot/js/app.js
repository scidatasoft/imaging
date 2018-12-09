var FileInfo = (function () {
    //function FileInfo(name, file) {
    function FileInfo(file) {
        //this.name = name;
        this.file = file;
    }
    return FileInfo;
}());

var app = angular.module('app', []);
app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);

app.service('fileService', ['$http', function ($http) {
    this.uploadFile = function (fileInfo) {
        var fd = new FormData();
        //fd.append('name', fileInfo.name);
        fd.append('file', fileInfo.file);

        return $http.post('/files/upload', fd, {
            transformRequest: angular.identity,
            headers: {
                'Content-Type': undefined
            }
        });
    }
}]);

app.controller('fileCtrl', ['$scope', 'fileService', function ($scope, fileService) {

    $scope.uploadFile = function () {
        $scope.showUploadStatus = false;
        $scope.showUploadedData = false;

        //var fileInfo = new FileInfo($scope.name, $scope.curriculumVitae);
        var fileInfo = new FileInfo($scope.file);

        fileService.uploadFile(fileInfo).then(function (response) { // success
            if (response.status == 200) {
                $scope.uploadStatus = "File created sucessfully.";
                $scope.uploadedData = response.data;
                $scope.showUploadStatus = true;
                $scope.showUploadedData = true;
            }
        },
        function (response) { // failure
            $scope.uploadStatus = "File creation failed with status code: " + response.status;
            $scope.showUploadStatus = true;
            $scope.showUploadedData = false;
        });
    };
}]);