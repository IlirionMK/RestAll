class MenuItemModel {
  final int id;
  final String name;
  final double price;
  final String category;

  MenuItemModel({required this.id, required this.name, required this.price, required this.category});

  factory MenuItemModel.fromJson(Map<String, dynamic> json) {
    return MenuItemModel(
      id: json['id'],
      name: json['name'],
      price: double.parse(json['price'].toString()),
      category: json['category_name'] ?? 'Ogólne',
    );
  }
}