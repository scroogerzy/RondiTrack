import 'package:flutter_riverpod/flutter_riverpod.dart';

enum AppDataMode { fake, api }

final appDataModeProvider = Provider<AppDataMode>(
  (ref) => AppDataMode.fake,
);
