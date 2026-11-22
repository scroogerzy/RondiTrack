import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
export 'package:ronditrack_mobile/core/config/app_config.dart'
    show appConfigProvider;
import 'package:ronditrack_mobile/core/theme/rondi_theme.dart';
import 'package:ronditrack_mobile/features/navigation/app_router.dart';

class RondiTrackApp extends ConsumerWidget {
  const new({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(appRouterProvider);
    return MaterialApp.router(
      title: 'RondiTrack',
      theme: RondiTheme.light(),
      darkTheme: RondiTheme.dark(),
      routerConfig: router,
    );
  }
}
