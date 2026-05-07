class OrderModel {
  final int id;
  final int tableId;
  final String status; // new, kitchen, ready, paid
  final List<dynamic> items;

  OrderModel({required this.id, required this.tableId, required this.status, this.items = const []});

  factory OrderModel.fromJson(Map<String, dynamic> json) {
    return OrderModel(
      id: json['id'],
      tableId: json['table_id'],
      status: json['status'],
      items: json['items'] ?? [],
    );
  }
}