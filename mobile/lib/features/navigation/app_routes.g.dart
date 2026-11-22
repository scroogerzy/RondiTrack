// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_routes.dart';

// **************************************************************************
// GoRouterGenerator
// **************************************************************************

List<RouteBase> get $appRoutes => [
  $appShellRouteData,
  $signInRoute,
  $joinRoute,
];

RouteBase get $appShellRouteData => StatefulShellRouteData.$route(
  factory: $AppShellRouteDataExtension._fromState,
  branches: [
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/home',
          hasOverriddenOnExit: false,
          factory: $HomeRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/stokvels',
          hasOverriddenOnExit: false,
          factory: $StokvelsRoute._fromState,
          routes: [
            GoRouteData.$route(
              path: ':id',
              hasOverriddenOnExit: false,
              factory: $StokvelDetailRoute._fromState,
              routes: [
                GoRouteData.$route(
                  path: 'cycles/:cycleId',
                  hasOverriddenOnExit: false,
                  factory: $StokvelCycleRoute._fromState,
                  routes: [
                    GoRouteData.$route(
                      path: 'payout-order',
                      hasOverriddenOnExit: false,
                      factory: $PayoutOrderRoute._fromState,
                    ),
                  ],
                ),
              ],
            ),
          ],
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/activity',
          hasOverriddenOnExit: false,
          factory: $ActivityRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/profile',
          hasOverriddenOnExit: false,
          factory: $ProfileRoute._fromState,
        ),
      ],
    ),
  ],
);

extension $AppShellRouteDataExtension on AppShellRouteData {
  static AppShellRouteData _fromState(GoRouterState state) =>
      const AppShellRouteData();
}

mixin $HomeRoute on GoRouteData {
  static HomeRoute _fromState(GoRouterState state) => const HomeRoute();

  @override
  String get location => GoRouteData.$location('/home');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $StokvelsRoute on GoRouteData {
  static StokvelsRoute _fromState(GoRouterState state) => const StokvelsRoute();

  @override
  String get location => GoRouteData.$location('/stokvels');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $StokvelDetailRoute on GoRouteData {
  static StokvelDetailRoute _fromState(GoRouterState state) =>
      StokvelDetailRoute(id: state.pathParameters['id']!);

  StokvelDetailRoute get _self => this as StokvelDetailRoute;

  @override
  String get location =>
      GoRouteData.$location('/stokvels/${Uri.encodeComponent(_self.id)}');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $StokvelCycleRoute on GoRouteData {
  static StokvelCycleRoute _fromState(GoRouterState state) => StokvelCycleRoute(
    id: state.pathParameters['id']!,
    cycleId: state.pathParameters['cycleId']!,
  );

  StokvelCycleRoute get _self => this as StokvelCycleRoute;

  @override
  String get location => GoRouteData.$location(
    '/stokvels/${Uri.encodeComponent(_self.id)}/cycles/${Uri.encodeComponent(_self.cycleId)}',
  );

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $PayoutOrderRoute on GoRouteData {
  static PayoutOrderRoute _fromState(GoRouterState state) => PayoutOrderRoute(
    id: state.pathParameters['id']!,
    cycleId: state.pathParameters['cycleId']!,
  );

  PayoutOrderRoute get _self => this as PayoutOrderRoute;

  @override
  String get location => GoRouteData.$location(
    '/stokvels/${Uri.encodeComponent(_self.id)}/cycles/${Uri.encodeComponent(_self.cycleId)}/payout-order',
  );

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $ActivityRoute on GoRouteData {
  static ActivityRoute _fromState(GoRouterState state) => const ActivityRoute();

  @override
  String get location => GoRouteData.$location('/activity');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $ProfileRoute on GoRouteData {
  static ProfileRoute _fromState(GoRouterState state) => const ProfileRoute();

  @override
  String get location => GoRouteData.$location('/profile');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $signInRoute => GoRouteData.$route(
  path: '/sign-in',
  hasOverriddenOnExit: false,
  factory: $SignInRoute._fromState,
);

mixin $SignInRoute on GoRouteData {
  static SignInRoute _fromState(GoRouterState state) =>
      SignInRoute(from: state.uri.queryParameters['from']);

  SignInRoute get _self => this as SignInRoute;

  @override
  String get location => GoRouteData.$location(
    '/sign-in',
    queryParams: {if (_self.from != null) 'from': _self.from},
  );

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $joinRoute => GoRouteData.$route(
  path: '/join/:code',
  hasOverriddenOnExit: false,
  factory: $JoinRoute._fromState,
);

mixin $JoinRoute on GoRouteData {
  static JoinRoute _fromState(GoRouterState state) =>
      JoinRoute(code: state.pathParameters['code']!);

  JoinRoute get _self => this as JoinRoute;

  @override
  String get location =>
      GoRouteData.$location('/join/${Uri.encodeComponent(_self.code)}');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}
