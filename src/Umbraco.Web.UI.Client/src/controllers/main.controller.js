
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $rootScope, $location, $routeParams, $timeout, $http, $log, appState, treeService, notificationsService, userService, navigationService, historyService, updateChecker, assetsService, eventsService, umbRequestHelper, tmhDynamicLocale, localStorageService, tourService, editorService) {

    //the null is important because we do an explicit bool check on this in the view
    $scope.authenticated = null;
    $scope.touchDevice = appState.getGlobalState("touchDevice");
    $scope.editors = [];
    $scope.overlay = {};
    
    $scope.removeNotification = function (index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function (event) {
        //only close dialogs if non-link and non-buttons are clicked
        var el = event.target.nodeName;
        var els = ["INPUT", "A", "BUTTON"];

        if (els.indexOf(el) >= 0) { return; }

        var parents = $(event.target).parents("a,button");
        if (parents.length > 0) {
            return;
        }

        //SD: I've updated this so that we don't close the dialog when clicking inside of the dialog
        var nav = $(event.target).parents("#dialog");
        if (nav.length === 1) {
            return;
        }

        eventsService.emit("app.closeDialogs", event);
    };

    var evts = [];

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function () {
        $scope.authenticated = null;
        $scope.user = null;
    }));

    evts.push(eventsService.on("app.userRefresh", function(evt) {
        userService.refreshCurrentUser().then(function(data) {
            $scope.user = data;

            //Load locale file
            if ($scope.user.locale) {
                tmhDynamicLocale.set($scope.user.locale);
            }
        });
    }));

    //when the app is ready/user is logged in, setup the data
    evts.push(eventsService.on("app.ready", function (evt, data) {

        $scope.authenticated = data.authenticated;
        $scope.user = data.user;

        updateChecker.check().then(function (update) {
            if (update && update !== "null") {
                if (update.type !== "None") {
                    var notification = {
                        headline: "Update available",
                        message: "Click to download",
                        sticky: true,
                        type: "info",
                        url: update.url
                    };
                    notificationsService.add(notification);
                }
            }
        });

        //if the user has changed we need to redirect to the root so they don't try to continue editing the
        //last item in the URL (NOTE: the user id can equal zero, so we cannot just do !data.lastUserId since that will resolve to true)
        if (data.lastUserId !== undefined && data.lastUserId !== null && data.lastUserId !== data.user.id) {
            $location.path("/").search("");
            historyService.removeAll();
            treeService.clearCache();

            //if the user changed, clearout local storage too - could contain sensitive data
            localStorageService.clearAll();
        }

        //if this is a new login (i.e. the user entered credentials), then clear out local storage - could contain sensitive data
        if (data.loginType === "credentials") {
            localStorageService.clearAll();
        }

        //Load locale file
        if ($scope.user.locale) {
            tmhDynamicLocale.set($scope.user.locale);
        }

    }));

    evts.push(eventsService.on("app.ysod", function (name, error) {
        $scope.ysodOverlay = {
            view: "ysod",
            error: error,
            show: true
        };
    }));

    // events for drawer
    // manage the help dialog by subscribing to the showHelp appState
    $scope.drawer = {};
    evts.push(eventsService.on("appState.drawerState.changed", function (e, args) {
        // set view
        if (args.key === "view") {
            $scope.drawer.view = args.value;
        }
        // set custom model
        if (args.key === "model") {
            $scope.drawer.model = args.value;
        }
        // show / hide drawer
        if (args.key === "showDrawer") {
            $scope.drawer.show = args.value;
        }
    }));

    // events for overlays
    evts.push(eventsService.on("appState.overlay", function (name, args) {
        $scope.overlay = args;
    }));
    
    // events for tours
    evts.push(eventsService.on("appState.tour.start", function (name, args) {
        $scope.tour = args;
        $scope.tour.show = true;
    }));

    evts.push(eventsService.on("appState.tour.end", function () {
        $scope.tour = null;
    }));

    evts.push(eventsService.on("appState.tour.complete", function () {
        $scope.tour = null;
    }));

    // events for backdrop
    evts.push(eventsService.on("appState.backdrop", function (name, args) {
        $scope.backdrop = args;
    }));

    evts.push(eventsService.on("appState.editors.add", function (name, args) {
        $scope.editors = args.editors;
    }));

    evts.push(eventsService.on("appState.editors.remove", function (name, args) {
        $scope.editors = args.editors;
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}


//register it
angular.module('umbraco').controller("Umbraco.MainController", MainController).
    config(function (tmhDynamicLocaleProvider) {
        //Set url for locale files
        tmhDynamicLocaleProvider.localeLocationPattern('lib/angular-i18n/angular-locale_{{locale}}.js');
    });
