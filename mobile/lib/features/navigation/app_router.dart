// Preserve the router's explicit setup and refresh sequence.
// ignore_for_file: cascade_invocations
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';
import 'package:ronditrack_mobile/features/navigation/app_routes.dart';

class AuthRefreshNotifier extends ChangeNotifier {
  void refresh() => notifyListeners();
}

final authRefreshListenableProvider = Provider<AuthRefreshNotifier>((ref) {
  final notifier = AuthRefreshNotifier();
  ref.listen(fakeAuthProvider, (previous, next) => notifier.refresh());
  ref.onDispose(notifier.dispose);
  return notifier;
});

final appRouterProvider = Provider<GoRouter>((ref) {
  final refresh = ref.watch(authRefreshListenableProvider);
  final router = GoRouter(
    routes: $appRoutes,
    initialLocation: '/home',
    refreshListenable: refresh,
    redirect: (context, state) {
      final auth = ref.read(fakeAuthProvider);
      return authRedirect(
        isSignedIn: auth.isSignedIn,
        location: state.uri.path,
        rememberedLocation: state.uri.queryParameters['from'],
      );
    },
  );
  ref.onDispose(router.dispose);
  return router;
});
