import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/app/app.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  const nativeFlavor = String.fromEnvironment('APP_FLAVOR');
  final config = AppConfig.load(nativeFlavor: nativeFlavor);
  runApp(
    ProviderScope(
      overrides: [appConfigProvider.overrideWithValue(config)],
      child: const RondiTrackApp(),
    ),
  );
}
