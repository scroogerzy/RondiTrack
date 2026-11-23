import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_detail.dart';

abstract interface class HomeRepository {
  Future<HomeResponse> loadHome();
  Future<StokvelDetail?> loadStokvel(String stokvelId);
}
