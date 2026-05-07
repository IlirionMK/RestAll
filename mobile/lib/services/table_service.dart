import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/table_model.dart';

class TableService {
  final String baseUrl = "http://10.0.2.2:8000/api";
  final _storage = const FlutterSecureStorage();

  // Pobieranie wszystkich stolików (RES-07)
  Future<List<TableModel>> fetchTables() async {
    final token = await _storage.read(key: 'token');

    final response = await http.get(
      Uri.parse('$baseUrl/tables'),
      headers: {
        'Accept': 'application/json',
        'Authorization': 'Bearer $token', // Autoryzacja wymagana przez BSI
      },
    );

    if (response.statusCode == 200) {
      List<dynamic> body = jsonDecode(response.body);
      return body.map((dynamic item) => TableModel.fromJson(item)).toList();
    } else {
      throw Exception("Nie udało się pobrać stolików");
    }
  }

  // Zmiana statusu stolika (RES-08)
  Future<bool> updateTableStatus(int id, String status) async {
    final token = await _storage.read(key: 'token');

    final response = await http.patch(
      Uri.parse('$baseUrl/tables/$id/status'),
      headers: {
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: {'status': status},
    );

    return response.statusCode == 200;
  }
}