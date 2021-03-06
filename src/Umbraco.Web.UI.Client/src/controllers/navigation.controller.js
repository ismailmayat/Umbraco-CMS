
/**
 * @ngdoc controller
 * @name Umbraco.NavigationController
 * @function
 *
 * @description
 * Handles the section area of the app
 *
 * @param {navigationService} navigationService A reference to the navigationService
 */
function NavigationController($scope, $rootScope, $location, $log, $q, $routeParams, $timeout, treeService, appState, navigationService, keyboardService, dialogService, historyService, eventsService, sectionResource, angularHelper, languageResource, contentResource) {

    //this is used to trigger the tree to start loading once everything is ready
    var treeInitPromise = $q.defer();

    $scope.treeApi = {};

    //Bind to the main tree events
    $scope.onTreeInit = function () {

        $scope.treeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);

        //when a tree is loaded into a section, we need to put it into appState
        $scope.treeApi.callbacks.treeLoaded(function (args) {
            appState.setTreeState("currentRootNode", args.tree);
        });

        //when a tree node is synced this event will fire, this allows us to set the currentNode
        $scope.treeApi.callbacks.treeSynced(function (args) {

            if (args.activate === undefined || args.activate === true) {
                //set the current selected node
                appState.setTreeState("selectedNode", args.node);
                //when a node is activated, this is the same as clicking it and we need to set the
                //current menu item to be this node as well.
                appState.setMenuState("currentNode", args.node);
            }
        });

        //this reacts to the options item in the tree
        $scope.treeApi.callbacks.treeOptionsClick(function (args) {
            args.event.stopPropagation();
            args.event.preventDefault();

            //Set the current action node (this is not the same as the current selected node!)
            appState.setMenuState("currentNode", args.node);

            if (args.event && args.event.altKey) {
                args.skipDefault = true;
            }

            navigationService.showMenu(args);
        });

        $scope.treeApi.callbacks.treeNodeAltSelect(function (args) {
            args.event.stopPropagation();
            args.event.preventDefault();

            args.skipDefault = true;
            navigationService.showMenu(args);
        });

        //this reacts to tree items themselves being clicked
        //the tree directive should not contain any handling, simply just bubble events
        $scope.treeApi.callbacks.treeNodeSelect(function (args) {
            var n = args.node;
            args.event.stopPropagation();
            args.event.preventDefault();

            if (n.metaData && n.metaData["jsClickCallback"] && angular.isString(n.metaData["jsClickCallback"]) && n.metaData["jsClickCallback"] !== "") {
                //this is a legacy tree node!
                var jsPrefix = "javascript:";
                var js;
                if (n.metaData["jsClickCallback"].startsWith(jsPrefix)) {
                    js = n.metaData["jsClickCallback"].substr(jsPrefix.length);
                }
                else {
                    js = n.metaData["jsClickCallback"];
                }
                try {
                    var func = eval(js);
                    //this is normally not necessary since the eval above should execute the method and will return nothing.
                    if (func != null && (typeof func === "function")) {
                        func.call();
                    }
                }
                catch (ex) {
                    $log.error("Error evaluating js callback from legacy tree node: " + ex);
                }
            }
            else if (n.routePath) {
                //add action to the history service
                historyService.add({ name: n.name, link: n.routePath, icon: n.icon });

                //put this node into the tree state
                appState.setTreeState("selectedNode", args.node);
                //when a node is clicked we also need to set the active menu node to this node
                appState.setMenuState("currentNode", args.node);

                //not legacy, lets just set the route value and clear the query string if there is one.
                $location.path(n.routePath);
                navigationService.clearSearch();
            }
            else if (n.section) {
                $location.path(n.section);
                navigationService.clearSearch();
            }

            navigationService.hideNavigation();
        });

        return treeInitPromise.promise;
    }

    //TODO: Remove this, this is not healthy
    // Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;
    // TODO: Remove this, this is not healthy
    // it is less than ideal to be passing in the navigationController scope to something else to be used as it's scope,
    // this is going to lead to problems/confusion. I really don't think passing scope's around is very good practice.
    $rootScope.nav = navigationService;

    //set up our scope vars
    $scope.showContextMenuDialog = false;
    $scope.showContextMenu = false;
    $scope.showSearchResults = false;
    $scope.menuDialogTitle = null;
    $scope.menuActions = [];
    $scope.menuNode = null;
    $scope.languages = [];
    $scope.selectedLanguage = {};
    $scope.page = {};
    $scope.page.languageSelectorIsOpen = false;

    $scope.currentSection = null;
    $scope.customTreeParams = null;
    $scope.treeCacheKey = "_";
    $scope.showNavigation = appState.getGlobalState("showNavigation");
    // tracks all expanded paths so when the language is switched we can resync it with the already loaded paths
    var expandedPaths = [];

    //trigger search with a hotkey:
    keyboardService.bind("ctrl+shift+s", function () {
        navigationService.showSearch();
    });

    //trigger dialods with a hotkey:
    keyboardService.bind("esc", function () {
        eventsService.emit("app.closeDialogs");
    });

    $scope.selectedId = navigationService.currentId;

    var evts = [];

    //Listen for global state changes
    evts.push(eventsService.on("appState.globalState.changed", function (e, args) {
        if (args.key === "showNavigation") {
            $scope.showNavigation = args.value;
        }
    }));

    //Listen for menu state changes
    evts.push(eventsService.on("appState.menuState.changed", function (e, args) {
        if (args.key === "showMenuDialog") {
            $scope.showContextMenuDialog = args.value;
        }
        if (args.key === "showMenu") {
            $scope.showContextMenu = args.value;
        }
        if (args.key === "dialogTitle") {
            $scope.menuDialogTitle = args.value;
        }
        if (args.key === "menuActions") {
            $scope.menuActions = args.value;
        }
        if (args.key === "currentNode") {
            $scope.menuNode = args.value;
        }
    }));

    //Listen for section state changes
    evts.push(eventsService.on("appState.treeState.changed", function (e, args) {
        var f = args;
        if (args.value.root && args.value.root.metaData.containsTrees === false) {
            $rootScope.emptySection = true;
        }
        else {
            $rootScope.emptySection = false;
        }
    }));

    //Listen for section state changes
    evts.push(eventsService.on("appState.sectionState.changed", function (e, args) {
        //section changed
        if (args.key === "currentSection" && $scope.currentSection != args.value) {
            $scope.currentSection = args.value;

            //load the tree
            configureTreeAndLanguages();
            $scope.treeApi.load({ section: $scope.currentSection, customTreeParams: $scope.customTreeParams, cacheKey: $scope.treeCacheKey });

        }
        //show/hide search results
        if (args.key === "showSearchResults") {
            $scope.showSearchResults = args.value;
        }

    }));

    // Listen for language updates
    evts.push(eventsService.on("editors.languages.languageDeleted", function (e, args) {
        loadLanguages().then(function (languages) {
            $scope.languages = languages;
        });
    }));

    evts.push(eventsService.on("editors.languages.languageCreated", function (e, args) {
        loadLanguages().then(function (languages) {
            $scope.languages = languages;
        });
    }));

    //This reacts to clicks passed to the body element which emits a global call to close all dialogs
    evts.push(eventsService.on("app.closeDialogs", function (event) {
        if (appState.getGlobalState("stickyNavigation")) {
            navigationService.hideNavigation();
            //TODO: don't know why we need this? - we are inside of an angular event listener.
            angularHelper.safeApply($scope);
        }
    }));

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function () {
        $scope.authenticated = false;
    }));

    //when the application is ready and the user is authorized setup the data
    evts.push(eventsService.on("app.ready", function (evt, data) {
        init();
    }));

    /**
     * Based on the current state of the application, this configures the scope variables that control the main tree and language drop down
     */
    function configureTreeAndLanguages() {

        //create the custom query string param for this tree, this is currently only relevant for content
        if ($scope.currentSection === "content") {

            //must use $location here because $routeParams isn't available until after the route change
            var mainCulture = $location.search().mculture;
            //select the current language if set in the query string
            if (mainCulture && $scope.languages && $scope.languages.length > 1) {
                var found = _.find($scope.languages, function (l) {
                    return l.culture.toLowerCase() === mainCulture.toLowerCase();
                });
                if (found) {
                    //set the route param
                    $scope.selectedLanguage = found;
                }
            }

            var queryParams = {};
            if ($scope.selectedLanguage && $scope.selectedLanguage.culture) {
                queryParams["culture"] = $scope.selectedLanguage.culture;
            }
            var queryString = $.param(queryParams); //create the query string from the params object
        }

        if (queryString) {
            $scope.customTreeParams = queryString;
            $scope.treeCacheKey = queryString; // this tree uses caching but we need to change it's cache key per lang
        }
        else {
            $scope.treeCacheKey = "_"; // this tree uses caching, there's no lang selected so use the default
        }

    }

    /**
     * Called when the app is ready and sets up the navigation (should only be called once)
     */
    function init() {

        $scope.authenticated = true;

        var navInit = false;

        //$routeParams will be populated after $routeChangeSuccess since this controller is used outside ng-view,
        //* we listen for the first route change with a section to setup the navigation.
        //* we listen for all route changes to track the current section.
        $rootScope.$on('$routeChangeSuccess', function () {

            //only continue if there's a section available
            if ($routeParams.section) {

                if (!navInit) {
                    navInit = true;
                    initNav();
                }
                else {
                    //keep track of the current section, when it changes change the state, and we listen for that event change above
                    if ($scope.currentSection != $routeParams.section) {
                        appState.setSectionState("currentSection", $routeParams.section);
                    }
                }
            }
        });
    }

    /**
     * This loads the language data, if the are no variant content types configured this will return no languages
     */
    function loadLanguages() {

        return contentResource.allowsCultureVariation().then(function (b) {
            if (b === true) {
                return languageResource.getAll()
            } else {
                return $q.when([]); //resolve an empty collection
            }
        });
    }

    /**
     * Called once during init to initialize the navigation/tree/languages
     */
    function initNav() {
        // load languages
        loadLanguages().then(function (languages) {

            $scope.languages = languages;

            if ($scope.languages.length > 1) {
                var defaultLang = _.find($scope.languages, function (l) {
                    return l.isDefault;
                });
                //if there's already one set, check if it exists
                var currCulture = null;
                var mainCulture = $location.search().mculture;
                if (mainCulture) {
                    currCulture = _.find($scope.languages, function (l) {
                        return l.culture.toLowerCase() === mainCulture.toLowerCase();
                    });
                }
                if (!currCulture) {
                    $location.search("mculture", defaultLang ? defaultLang.culture : null);
                }
            }

            $scope.currentSection = $routeParams.section;

            configureTreeAndLanguages();

            //resolve the tree promise, set it's property values for loading the tree which will make the tree load
            treeInitPromise.resolve({
                section: $scope.currentSection,
                customTreeParams: $scope.customTreeParams,
                cacheKey: $scope.treeCacheKey,

                //because angular doesn't return a promise for the resolve method, we need to resort to some hackery, else
                //like normal JS promises we could do resolve(...).then()
                onLoaded: function () {

                    //the nav is ready, let the app know
                    eventsService.emit("app.navigationReady", { treeApi: $scope.treeApi });
                    //finally set the section state
                    appState.setSectionState("currentSection", $routeParams.section);
                }
            });
        });
    }
    function nodeExpandedHandler(args) {
        //store the reference to the expanded node path
        if (args.node) {
            treeService._trackExpandedPaths(args.node, expandedPaths);
        }
    }

    $scope.selectLanguage = function (language) {

        $location.search("mculture", language.culture);

        // close the language selector
        $scope.page.languageSelectorIsOpen = false;

        configureTreeAndLanguages(); //re-bind language to the query string and update the tree params

        //reload the tree with it's updated querystring args
        $scope.treeApi.load({ section: $scope.currentSection, customTreeParams: $scope.customTreeParams, cacheKey: $scope.treeCacheKey }).then(function () {

            //re-sync to currently edited node
            var currNode = appState.getTreeState("selectedNode");
            //create the list of promises
            var promises = [];
            //starting with syncing to the currently selected node if there is one
            if (currNode) {
                var path = treeService.getPath(currNode);
                promises.push($scope.treeApi.syncTree({ path: path, activate: true }));
            }
            //TODO: If we want to keep all paths expanded ... but we need more testing since we need to deal with unexpanding
            //for (var i = 0; i < expandedPaths.length; i++) {
            //    promises.push($scope.treeApi.syncTree({ path: expandedPaths[i], activate: false, forceReload: true }));
            //}
            //execute them sequentially
            angularHelper.executeSequentialPromises(promises);
        });

    };

    //this reacts to the options item in the tree
    //TODO: migrate to nav service
    //TODO: is this used? 
    $scope.searchShowMenu = function (ev, args) {
        //always skip default
        args.skipDefault = true;
        navigationService.showMenu(args);
    };

    //TODO: migrate to nav service
    //TODO: is this used?
    $scope.searchHide = function () {
        navigationService.hideSearch();
    };

    //the below assists with hiding/showing the tree
    var treeActive = false;

    //Sets a service variable as soon as the user hovers the navigation with the mouse
    //used by the leaveTree method to delay hiding
    $scope.enterTree = function (event) {
        treeActive = true;
    };

    // Hides navigation tree, with a short delay, is cancelled if the user moves the mouse over the tree again
    $scope.leaveTree = function (event) {
        //this is a hack to handle IE touch events which freaks out due to no mouse events so the tree instantly shuts down
        if (!event) {
            return;
        }
        if (!appState.getGlobalState("touchDevice")) {
            treeActive = false;
            $timeout(function () {
                if (!treeActive) {
                    navigationService.hideTree();
                }
            }, 300);
        }
    };

    $scope.toggleLanguageSelector = function () {
        $scope.page.languageSelectorIsOpen = !$scope.page.languageSelectorIsOpen;
    };

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });
}

//register it
angular.module('umbraco').controller("Umbraco.NavigationController", NavigationController);
