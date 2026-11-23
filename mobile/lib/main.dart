import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'app/app.dart';
import 'core/config/app_config.dart';
import 'core/data/app_data_mode.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  const nativeFlavor = String.fromEnvironment('APP_FLAVOR');
  final config = AppConfig.load(nativeFlavor: nativeFlavor);

  runApp(
    ProviderScope(
      overrides: [
        appConfigProvider.overrideWithValue(config),
        appDataModeProvider.overrideWithValue(AppDataMode.api),
      ],
      child: const RondiTrackApp(),
    ),
  );
}
