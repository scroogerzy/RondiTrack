import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:ronditrack_mobile/features/activity/presentation/activity_screen.dart';
import 'package:ronditrack_mobile/features/auth/presentation/sign_in_screen.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_screen.dart';
import 'package:ronditrack_mobile/features/invites/presentation/invite_preview_screen.dart';
import 'package:ronditrack_mobile/features/navigation/app_shell.dart';
import 'package:ronditrack_mobile/features/profile/presentation/profile_screen.dart';
import 'package:ronditrack_mobile/features/payouts/presentation/payout_order_screen.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvel_cycle_screen.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvel_detail_screen.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvels_screen.dart';

part 'app_routes.g.dart';

@TypedStatefulShellRoute<AppShellRouteData>(
  branches: <TypedStatefulShellBranch<StatefulShellBranchData>>[
    TypedStatefulShellBranch<HomeBranchData>(
      routes: <TypedRoute<RouteData>>[TypedGoRoute<HomeRoute>(path: '/home')],
    ),
    TypedStatefulShellBranch<StokvelsBranchData>(
      routes: <TypedRoute<RouteData>>[
        TypedGoRoute<StokvelsRoute>(
          path: '/stokvels',
          routes: [
            TypedGoRoute<StokvelDetailRoute>(
              path: ':id',
              routes: [
                TypedGoRoute<StokvelCycleRoute>(
                  path: 'cycles/:cycleId',
                  routes: [
                    TypedGoRoute<PayoutOrderRoute>(path: 'payout-order'),
                  ],
                ),
              ],
            ),
          ],
        ),
      ],
    ),
    TypedStatefulShellBranch<ActivityBranchData>(
      routes: <TypedRoute<RouteData>>[
        TypedGoRoute<ActivityRoute>(path: '/activity'),
      ],
    ),
    TypedStatefulShellBranch<ProfileBranchData>(
      routes: <TypedRoute<RouteData>>[
        TypedGoRoute<ProfileRoute>(path: '/profile'),
      ],
    ),
  ],
)
class AppShellRouteData extends StatefulShellRouteData {
  const new();
  @override
  Widget builder(
    BuildContext context,
    GoRouterState state,
    StatefulNavigationShell navigationShell,
  ) => AppShell(navigationShell: navigationShell);
}

class HomeBranchData extends StatefulShellBranchData {
  const new();
}

class StokvelsBranchData extends StatefulShellBranchData {
  const new();
}

class ActivityBranchData extends StatefulShellBranchData {
  const new();
}

class ProfileBranchData extends StatefulShellBranchData {
  const new();
}

class HomeRoute extends GoRouteData with $HomeRoute {
  const new();
  @override
  Widget build(BuildContext context, GoRouterState state) => const HomeScreen();
}

class StokvelsRoute extends GoRouteData with $StokvelsRoute {
  const new();
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      const StokvelsScreen();
}

class StokvelDetailRoute extends GoRouteData with $StokvelDetailRoute {
  const new({required this.id});
  final String id;
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      StokvelDetailScreen(stokvelId: id);
}

class StokvelCycleRoute extends GoRouteData with $StokvelCycleRoute {
  const new({required this.id, required this.cycleId});
  final String id;
  final String cycleId;
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      StokvelCycleScreen(stokvelId: id, cycleId: cycleId);
}

class PayoutOrderRoute extends GoRouteData with $PayoutOrderRoute {
  const new({required this.id, required this.cycleId});
  final String id;
  final String cycleId;
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      PayoutOrderScreen(stokvelId: id, cycleId: cycleId);
}

class ActivityRoute extends GoRouteData with $ActivityRoute {
  const new();
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      const ActivityScreen();
}

class ProfileRoute extends GoRouteData with $ProfileRoute {
  const new();
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      const ProfileScreen();
}

@TypedGoRoute<SignInRoute>(path: '/sign-in')
class SignInRoute extends GoRouteData with $SignInRoute {
  const new({this.from});
  final String? from;
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      SignInScreen(from: from);
}

@TypedGoRoute<JoinRoute>(path: '/join/:code')
class JoinRoute extends GoRouteData with $JoinRoute {
  const new({required this.code});
  final String code;
  @override
  Widget build(BuildContext context, GoRouterState state) =>
      InvitePreviewScreen(code: code);
}
