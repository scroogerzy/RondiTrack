import 'package:flutter/material.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';

class RondiTheme {
  static const seed = Color(0xFF176B57);

  static ThemeData light() => ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(seedColor: seed),
    extensions: const [RondiStatusColors.light],
  );

  static ThemeData dark() => ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(
      seedColor: seed,
      brightness: Brightness.dark,
    ),
    extensions: const [RondiStatusColors.dark],
  );
}
