import 'package:ronditrack_mobile/features/home/domain/home_models.dart';

abstract interface class HomeRepository {
  Future<HomeResponse> loadHome();
  Future<StokvelHomeItem?> loadStokvel(String stokvelId);
}
