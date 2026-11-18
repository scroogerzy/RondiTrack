import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_screen.dart';

final appConfigProvider = Provider<AppConfig>(
  (ref) => throw StateError('AppConfig must be supplied by main().'),
);

class RondiTrackApp extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
    title: 'RondiTrack',
    theme: ThemeData(
      colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF176B57)),
      useMaterial3: true,
    ),
    home: const HomeScreen(),
  );
}
