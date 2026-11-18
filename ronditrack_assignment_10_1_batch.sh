#!/usr/bin/env bash
set -euo pipefail
ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT/mobile"
test "$(git -C "$ROOT" branch --show-current)" = "feature/assignment-10-1-mobile-foundations"

mkdir -p lib/app lib/core/config lib/core/money lib/features/home/{data,domain,presentation} lib/features/stokvel/domain test/core/money test/features/home config

cat > pubspec.yaml <<'YAML'
name: ronditrack_mobile
description: RondiTrack mobile application
publish_to: 'none'
version: 1.0.0+1
environment:
  sdk: ">=3.13.5 <4.0.0"
dependencies:
  flutter:
    sdk: flutter
  flutter_riverpod: 3.2.0
  riverpod_annotation: 4.0.0
  freezed_annotation: 3.1.0
dev_dependencies:
  flutter_test:
    sdk: flutter
  very_good_analysis: 11.0.0
  build_runner: 2.16.2
  riverpod_generator: 4.0.9
  riverpod_lint: 3.0.3
  freezed: 4.0.3
flutter:
  uses-material-design: true
YAML

cat > analysis_options.yaml <<'YAML'
include: package:very_good_analysis/analysis_options.yaml
analyzer:
  plugins:
    - riverpod_lint
  exclude:
    - "**/*.freezed.dart"
    - "**/*.g.dart"
    - build/**
    - android/**
    - ios/**
linter:
  rules:
    public_member_api_docs: false
YAML

cat > config/dev.json <<'JSON'
{"APP_ENV":"dev","API_BASE_URL":"https://dev-api.example.invalid"}
JSON
cat > config/staging.json <<'JSON'
{"APP_ENV":"staging","API_BASE_URL":"https://staging-api.example.invalid"}
JSON
cat > config/prod.json <<'JSON'
{"APP_ENV":"prod","API_BASE_URL":"https://api.example.invalid"}
JSON

cat > lib/core/config/app_config.dart <<'DART'
class AppConfig {
  const AppConfig(this.environment, this.apiBaseUrl);
  final String environment;
  final Uri apiBaseUrl;

  static AppConfig load(String expected) {
    const env = String.fromEnvironment('APP_ENV');
    const url = String.fromEnvironment('API_BASE_URL');
    if (env.isEmpty || url.isEmpty) {
      throw StateError('Missing APP_ENV or API_BASE_URL; use --dart-define-from-file.');
    }
    if (env != expected) throw StateError('Flavor/config mismatch: $expected vs $env.');
    final uri = Uri.tryParse(url);
    if (uri == null || !uri.hasScheme || uri.host.isEmpty) throw const FormatException('Invalid API URL.');
    if (env == 'prod' && uri.scheme != 'https') throw StateError('Production requires HTTPS.');
    return AppConfig(env, uri);
  }
}
DART

cat > lib/core/money/cents.dart <<'DART'
extension type const Cents(int value) implements int {
  String formatEnZa() {
    final digits = (value.abs() ~/ 100).toString();
    final chunks = <String>[];
    for (var end = digits.length; end > 0; end -= 3) {
      chunks.insert(0, digits.substring((end - 3).clamp(0, digits.length), end));
    }
    return 'R${value < 0 ? '-' : ''}${chunks.join('\u00a0')}.${(value.abs() % 100).toString().padLeft(2, '0')}';
  }

  static Cents? tryParse(String input) {
    var text = input.trim().replaceFirst(RegExp(r'^[Rr]\s*'), '').replaceAll('\u00a0', ' ');
    if (text.isEmpty || text.endsWith('.') || !RegExp(r'^\d[\d ]*(,\d{1,2})?$').hasMatch(text)) return null;
    final parts = text.split(',');
    final whole = int.tryParse(parts.first.replaceAll(' ', ''));
    if (whole == null) return null;
    final fraction = parts.length == 1 ? 0 : int.parse(parts[1].padRight(2, '0'));
    return Cents(whole * 100 + fraction);
  }
}
DART

cat > lib/features/stokvel/domain/stokvel_status.dart <<'DART'
enum StokvelRole {
  member('Member'), treasurer('Treasurer'), chairperson('Chairperson'), unknown('');
  const StokvelRole(this.apiValue);
  final String apiValue;
  static StokvelRole fromApi(String? value) => switch (value) {
    'Member' => member, 'Treasurer' => treasurer, 'Chairperson' => chairperson, _ => unknown,
  };
}

/// Client-derived lifecycle; not an API enum. The API has ClosedAtUtc and PayoutProcessedAtUtc.
enum CycleStatus { scheduled, open, closed, payoutProcessed, unknown }

/// Local display classification only; API Contribution has no status field.
enum ContributionStatus { recorded, unknown }

enum StokvelAction { pay, uploadProof, confirm, closeCycle, recordPayout }

CycleStatus deriveCycleStatus({
  required DateTime nowUtc, required DateTime startUtc, required DateTime endUtc,
  required DateTime? closedAtUtc, required DateTime? payoutProcessedAtUtc,
}) {
  if (payoutProcessedAtUtc != null) return CycleStatus.payoutProcessed;
  if (closedAtUtc != null) return CycleStatus.closed;
  if (nowUtc.isBefore(startUtc)) return CycleStatus.scheduled;
  if (!nowUtc.isAfter(endUtc)) return CycleStatus.open;
  return CycleStatus.unknown;
}

Set<StokvelAction> availableActions({
  required StokvelRole role, required CycleStatus cycleStatus,
  required ContributionStatus contributionStatus, required bool hasOwnContribution,
}) {
  if (role == StokvelRole.unknown || cycleStatus == CycleStatus.unknown) return const {};
  final manager = role == StokvelRole.treasurer || role == StokvelRole.chairperson;
  return switch ((manager, cycleStatus, hasOwnContribution, contributionStatus)) {
    (false, CycleStatus.open, false, _) => {StokvelAction.pay, StokvelAction.uploadProof},
    (false, CycleStatus.open, true, ContributionStatus.recorded) => {StokvelAction.uploadProof},
    (true, CycleStatus.open, _, _) => {StokvelAction.confirm, StokvelAction.closeCycle},
    (true, CycleStatus.closed, _, _) => {StokvelAction.recordPayout},
    _ => const {},
  };
}
DART

cat > lib/features/home/domain/home_models.dart <<'DART'
import 'package:freezed_annotation/freezed_annotation.dart';
part 'home_models.freezed.dart';
part 'home_models.g.dart';

@freezed
abstract class StokvelHomeItem with _$StokvelHomeItem {
  const factory StokvelHomeItem({
    required String stokvelId, required String name, required String role,
    required int? currentCycle, required int amountPaidThisCycleCents,
    required int amountOwedCents, required DateTime? nextPayoutDateUtc,
    required int arrearsCents,
  }) = _StokvelHomeItem;
  factory StokvelHomeItem.fromJson(Map<String, dynamic> json) => _$StokvelHomeItemFromJson(json);
}

@freezed
abstract class HomeResponse with _$HomeResponse {
  const factory HomeResponse({
    required String currentUserId, required List<StokvelHomeItem> stokvels,
  }) = _HomeResponse;
  factory HomeResponse.fromJson(Map<String, dynamic> json) => _$HomeResponseFromJson(json);
}
DART

cat > lib/features/home/data/home_repository.dart <<'DART'
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../domain/home_models.dart';
part 'home_repository.g.dart';

abstract interface class HomeRepository {
  Future<HomeResponse> loadHome();
  Future<StokvelHomeItem?> loadStokvel(String stokvelId);
}

enum DemoPersona { member, treasurer, chairperson }

class FakeHomeRepository implements HomeRepository {
  FakeHomeRepository({this.persona = DemoPersona.member});
  final DemoPersona persona;

  @override
  Future<HomeResponse> loadHome() async {
    final values = switch (persona) {
      DemoPersona.member => [
        _item('stokvel-sunrise','Sunrise Savings','Member',4,125000,25000,0),
        _item('stokvel-family','Family Circle','Member',null,0,0,15000),
      ],
      DemoPersona.treasurer => [
        _item('stokvel-sunrise','Sunrise Savings','Treasurer',4,150000,0,0),
        _item('stokvel-family','Family Circle','Member',4,50000,100000,25000),
      ],
      DemoPersona.chairperson => [
        _item('stokvel-community','Community Builders','Chairperson',2,200000,0,0),
      ],
    };
    return HomeResponse(
      currentUserId: persona == DemoPersona.member
        ? '10000000-0000-4000-8000-000000000001'
        : '20000000-0000-4000-8000-000000000002',
      stokvels: values,
    );
  }

  @override
  Future<StokvelHomeItem?> loadStokvel(String id) async {
    for (final item in (await loadHome()).stokvels) {
      if (item.stokvelId == id) return item;
    }
    return null;
  }

  static StokvelHomeItem _item(String id,String name,String role,int? cycle,int paid,int owed,int arrears) =>
    StokvelHomeItem(stokvelId:id,name:name,role:role,currentCycle:cycle,
      amountPaidThisCycleCents:paid,amountOwedCents:owed,
      nextPayoutDateUtc:null,arrearsCents:arrears);
}

@Riverpod(keepAlive: true)
HomeRepository homeRepository(Ref ref) => FakeHomeRepository();
DART

cat > lib/features/home/presentation/home_controller.dart <<'DART'
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../data/home_repository.dart';
import '../domain/home_models.dart';
part 'home_controller.g.dart';

@riverpod
class HomeController extends _$HomeController {
  @override
  Future<HomeResponse> build() => ref.watch(homeRepositoryProvider).loadHome();

  Future<void> refresh() async {
    state = const AsyncLoading<HomeResponse>().copyWithPrevious(state);
    try {
      state = AsyncData(await ref.read(homeRepositoryProvider).loadHome());
    } catch (error, stack) {
      state = AsyncError<HomeResponse>(error, stack).copyWithPrevious(state);
    }
  }
}

@riverpod
Future<StokvelHomeItem?> stokvelDetail(Ref ref, String stokvelId) =>
  ref.watch(homeRepositoryProvider).loadStokvel(stokvelId);
DART

cat > lib/features/home/presentation/home_screen.dart <<'DART'
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/money/cents.dart';
import 'home_controller.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final home = ref.watch(homeControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('RondiTrack')),
      body: RefreshIndicator(
        onRefresh: () => ref.read(homeControllerProvider.notifier).refresh(),
        child: home.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, stack) => ListView(children: [
            const SizedBox(height: 140),
            const Center(child: Text('We could not load your stokvels.')),
            Center(child: TextButton(
              onPressed: () => ref.invalidate(homeControllerProvider),
              child: const Text('Retry'),
            )),
          ]),
          data: (data) => data.stokvels.isEmpty
            ? const ListView(children: [
                SizedBox(height: 160),
                Center(child: Text('You have no stokvel memberships yet.')),
              ])
            : ListView(padding: const EdgeInsets.all(16), children: [
                Text('Your stokvels', style: Theme.of(context).textTheme.headlineSmall),
                for (final item in data.stokvels)
                  Card(child: ListTile(
                    title: Text(item.name),
                    subtitle: Text('${item.role} · Cycle ${item.currentCycle ?? '—'}'),
                    trailing: Text(Cents(item.amountOwedCents).formatEnZa()),
                  )),
              ]),
        ),
      ),
    );
  }
}
DART

cat > lib/app/app.dart <<'DART'
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../features/home/presentation/home_screen.dart';

class RondiTrackApp extends StatelessWidget {
  const RondiTrackApp({super.key});
  @override
  Widget build(BuildContext context) => ProviderScope(child: MaterialApp(
    title: 'RondiTrack',
    theme: ThemeData(colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF176B57)),useMaterial3: true),
    home: const HomeScreen(),
  ));
}
DART

cat > lib/main.dart <<'DART'
import 'package:flutter/material.dart';
import 'app/app.dart';
import 'core/config/app_config.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  const flavor = String.fromEnvironment('APP_FLAVOR');
  final config = AppConfig.load(flavor);
  runApp(RondiTrackApp(key: ValueKey(config.environment)));
}
DART

cat > test/core/money/cents_test.dart <<'DART'
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';

void main() {
  test('parses accepted South African rand inputs', () {
    expect(Cents.tryParse('150')?.value, 15000);
    expect(Cents.tryParse('150,50')?.value, 15050);
    expect(Cents.tryParse('R150')?.value, 15000);
    expect(Cents.tryParse('R 1 500')?.value, 150000);
    expect(Cents.tryParse('1 500,50')?.value, 150050);
    expect(Cents.tryParse('1\u00a0500,50')?.value, 150050);
  });
  test('rejects ambiguous or invalid input', () {
    for (final value in ['1,500.50','1,2,3','10,555','10.','','letters','R','-10']) {
      expect(Cents.tryParse(value), isNull, reason: value);
    }
  });
  test('formats cents using NBSP grouping', () {
    expect(Cents(150050).formatEnZa(), 'R1\u00a0500.50');
    expect(Cents(15000).formatEnZa(), 'R150.00');
  });
}
DART

cat > test/features/home/home_controller_test.dart <<'DART'
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';

void main() {
  test('home uses a fresh container and fake repository', () async {
    final container = ProviderContainer(overrides: [
      homeRepositoryProvider.overrideWith((ref) => FakeHomeRepository()),
    ]);
    addTearDown(container.dispose);
    final result = await container.read(homeControllerProvider.future);
    expect(result.stokvels, hasLength(2));
  });
  test('detail is scoped to stokvel id and hides nonmember as missing', () async {
    final container = ProviderContainer(overrides: [
      homeRepositoryProvider.overrideWith((ref) => FakeHomeRepository()),
    ]);
    addTearDown(container.dispose);
    expect((await container.read(stokvelDetailProvider('stokvel-sunrise').future))?.name,
      'Sunrise Savings');
    expect(await container.read(stokvelDetailProvider('not-member').future), isNull);
  });
}
DART

cat > README.md <<'MD'
# RondiTrack Mobile — Assignment 10.1

## Contract notes
- Home: `GET /api/v1/me/home`.
- `HomeResponse`: `currentUserId`, `stokvels`.
- `StokvelHomeItem`: `stokvelId`, `name`, `role`, nullable `currentCycle`,
  `amountPaidThisCycleCents`, `amountOwedCents`, nullable `nextPayoutDateUtc`,
  `arrearsCents`.
- Money values are integer cents. `nextPayoutDateUtc` is nullable UTC date-time.
- Roles are per `StokvelMember` relationship, composite key `(UserId, StokvelId)`.
  Exact values: `Member`, `Treasurer`, `Chairperson`. Do not use JWT role claim
  as the per-stokvel role source.
- The API has no `CycleStatus` or `ContributionStatus` wire enum. Cycle has
  `ClosedAtUtc` and `PayoutProcessedAtUtc`; contribution is immutable and has
  no status. Local types are derived/presentation types, not API strings.
- Record contribution: `POST /api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions`;
  JSON fields `userId`, `amountInCents`, and `Idempotency-Key` header.
- `/me/home` resolves caller from `sub`, then `ClaimTypes.NameIdentifier`.

## Run and generate
`flutter pub get`
`dart run build_runner build --delete-conflicting-outputs`
`dart analyze --fatal-infos`
`flutter test`

## Flavors
Run with `--dart-define=APP_FLAVOR=dev --dart-define-from-file=config/dev.json`
(or staging/prod matching config). Replace `.example.invalid` URLs with
approved environment URLs. Missing/mismatched config fails fast; prod requires HTTPS.
Android Gradle and iOS schemes must both be configured before flavor launch.

## Architecture and code generation
Feature-first layout. Widgets depend on `HomeRepository`; fake implementation
is the Week 10 boundary and Week 11 should replace its provider binding.
Freezed generates immutable JSON models; Riverpod generates provider code.
Generated files are committed.

## Money
Integer-only cents. Accepts `150`, `150,50`, `R150`, `R 1 500`,
`1 500,50` and NBSP grouping. Rejects `1,500.50`, 3 decimal places,
trailing period, empty, letters and negative values. Grouping uses NBSP.

## Status evolution
When API adds a status, add its exact wire value, display label, exhaustive
action policy and tests. Unknown values remain unknown and grant no privileged actions.

## Provider lifetimes
Home AsyncNotifier and stokvel detail family are auto-disposed. Detail is
keyed by stokvel ID and returns null for a missing/nonmember item. On sign-out,
invalidate home and session-scoped providers. No widget imports fake data.

## Demo personas
Fake data includes Member, Treasurer and Chairperson personas. These are
client-only fixtures, not production or API seed identities.

## Gaps/evidence
API integration is deferred. No explicit contribution proof status, cycle status
or Payout entity exists in the supplied backend. iOS schemes, native flavor IDs,
launcher screenshots, device evidence and CI red/green proof remain to be done
and must be captured from actual builds.
MD

# Android flavors: insert into the generated Kotlin DSL android block.
python3 - <<'PY'
from pathlib import Path
p = Path('android/app/build.gradle.kts')
s = p.read_text()
if 'flavorDimensions += "environment"' not in s:
    marker = 'android {'
    if marker not in s:
        raise SystemExit('Android Gradle android block not found')
    block = (
        '    flavorDimensions += "environment"\n'
        '    productFlavors {\n'
        '        create("dev") { dimension = "environment"; applicationIdSuffix = ".dev"; resValue("string", "app_name", "RondiTrack Dev") }\n'
        '        create("staging") { dimension = "environment"; applicationIdSuffix = ".staging"; resValue("string", "app_name", "RondiTrack Staging") }\n'
        '        create("prod") { dimension = "environment"; resValue("string", "app_name", "RondiTrack") }\n'
        '    }\n'
    )
    p.write_text(s.replace(marker, marker + '\n' + block, 1))
manifest = Path('android/app/src/main/AndroidManifest.xml')
if manifest.exists():
    p.write_text(p.read_text().replace('android:label="ronditrack_mobile"', 'android:label="@string/app_name"'))
PY

flutter pub get
dart run build_runner build --delete-conflicting-outputs
dart format lib test
dart analyze --fatal-infos
flutter test
git -C "$ROOT" status --short
