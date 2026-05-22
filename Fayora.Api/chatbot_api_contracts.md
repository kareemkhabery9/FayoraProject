# Fayora Chatbot API & UI Card Contracts

This document contains the exact JSON schemas, models, and type definitions for the Chatbot API. It serves as a guide for both Backend and Frontend developers to align on the response structure.

---

## 1. Main Response Payload

When the frontend calls `POST /api/Chatbot`, the server returns the following JSON object:

```json
{
  "sessionId": "acd36eee-388f-44a7-86e1-f6f21a533614",
  "response": {
    "text": "الرد النصي من الذكاء الاصطناعي...",
    "cards": [],
    "suggestions": [],
    "map": null
  }
}
```

---

## 2. Card Model Schema (`cards` array)

Each object inside the `cards` list has the following properties:

| Property | Type | Description |
| :--- | :--- | :--- |
| `id` | `string` | The unique identifier (GUID or string integer) of the item. |
| `type` | `string` | The card category. Must be one of: `'accommodation'`, `'package'`, `'booking'`, `'landmark'`. |
| `title` | `string` | The title of the accommodation, tour package, booking, or landmark. |
| `description` | `string` | Short summary, highlights, or status of the item. |
| `price` | `number` | Numeric value (e.g., `1200.00`). Interpretation depends on `type` (see below). |
| `imageUrl` | `string` | CDN URL for the main image of the item. |
| `rating` | `number` | Average rating value (from `0.0` to `5.0`). |
| `detailUrl` | `string` | The API path to fetch details or route to in the app (e.g., `"/api/Tourist/accommodation/{id}"`). |

### Card Type Behaviors:
1. **`accommodation`**: Representing hotels/apartments. `price` represents price per night.
2. **`package`**: Tour guide packages. `price` represents price per adult.
3. **`booking`**: User bookings. `price` represents total booking cost; `description` contains dates and status.
4. **`landmark`**: Touristic places/destinations. `price` is omitted or returned as `0`/`null`.

---

## 3. Map Schema (`map` object)

Returned only when the user requests directions or landmark locations.

| Property | Type | Description |
| :--- | :--- | :--- |
| `title` | `string` | Destination name (e.g., `"بحيرة قارون"`). |
| `duration` | `string` | Human-readable travel duration (e.g., `"45 mins"`). |
| `distance` | `string` | Distance string (e.g., `"52.3 km"`). |
| `startLatitude` | `number` | Departure latitude coordinate. |
| `startLongitude`| `number` | Departure longitude coordinate. |
| `endLatitude` | `number` | Destination latitude coordinate. |
| `endLongitude` | `number` | Destination longitude coordinate. |
| `routeDescription` | `string` | Instructions or summary of the path. |

---

## 4. Frontend Code Snippets (Ready to Use)

### 🔹 Dart / Flutter Class Model
```dart
class ChatbotResponseDto {
  final String sessionId;
  final ChatbotPayload response;

  ChatbotResponseDto({required this.sessionId, required this.response});

  factory ChatbotResponseDto.fromJson(Map<String, dynamic> json) {
    return ChatbotResponseDto(
      sessionId: json['sessionId'] ?? '',
      response: ChatbotPayload.fromJson(json['response'] ?? {}),
    );
  }
}

class ChatbotPayload {
  final String text;
  final List<ChatbotCard> cards;
  final List<String> suggestions;
  final ChatbotMap? map;

  ChatbotPayload({
    required this.text,
    required this.cards,
    required this.suggestions,
    this.map,
  });

  factory ChatbotPayload.fromJson(Map<String, dynamic> json) {
    return ChatbotPayload(
      text: json['text'] ?? '',
      cards: (json['cards'] as List? ?? [])
          .map((item) => ChatbotCard.fromJson(item))
          .toList(),
      suggestions: List<String>.from(json['suggestions'] ?? []),
      map: json['map'] != null ? ChatbotMap.fromJson(json['map']) : null,
    );
  }
}

class ChatbotCard {
  final String id;
  final String type; // 'accommodation' | 'package' | 'booking' | 'landmark'
  final String title;
  final String description;
  final double price;
  final String imageUrl;
  final double rating;
  final String detailUrl;

  ChatbotCard({
    required this.id,
    required this.type,
    required this.title,
    required this.description,
    required this.price,
    required this.imageUrl,
    required this.rating,
    required this.detailUrl,
  });

  factory ChatbotCard.fromJson(Map<String, dynamic> json) {
    return ChatbotCard(
      id: json['id']?.toString() ?? '',
      type: json['type'] ?? '',
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      price: (json['price'] as num?)?.toDouble() ?? 0.0,
      imageUrl: json['imageUrl'] ?? '',
      rating: (json['rating'] as num?)?.toDouble() ?? 0.0,
      detailUrl: json['detailUrl'] ?? '',
    );
  }
}

class ChatbotMap {
  final String title;
  final String duration;
  final String distance;
  final double startLatitude;
  final double startLongitude;
  final double endLatitude;
  final double endLongitude;
  final String routeDescription;

  ChatbotMap({
    required this.title,
    required this.duration,
    required this.distance,
    required this.startLatitude,
    required this.startLongitude,
    required this.endLatitude,
    required this.endLongitude,
    required this.routeDescription,
  });

  factory ChatbotMap.fromJson(Map<String, dynamic> json) {
    return ChatbotMap(
      title: json['title'] ?? '',
      duration: json['duration'] ?? '',
      distance: json['distance'] ?? '',
      startLatitude: (json['startLatitude'] as num?)?.toDouble() ?? 0.0,
      startLongitude: (json['startLongitude'] as num?)?.toDouble() ?? 0.0,
      endLatitude: (json['endLatitude'] as num?)?.toDouble() ?? 0.0,
      endLongitude: (json['endLongitude'] as num?)?.toDouble() ?? 0.0,
      routeDescription: json['routeDescription'] ?? '',
    );
  }
}
```

### 🔹 TypeScript / React Native Type definitions
```typescript
export interface ChatbotResponseDto {
  sessionId: string;
  response: ChatbotPayload;
}

export interface ChatbotPayload {
  text: string;
  cards: ChatbotCard[];
  suggestions: string[];
  map: ChatbotMap | null;
}

export interface ChatbotCard {
  id: string;
  type: 'accommodation' | 'package' | 'booking' | 'landmark';
  title: string;
  description: string;
  price: number;
  imageUrl: string;
  rating: number;
  detailUrl: string;
}

export interface ChatbotMap {
  title: string;
  duration: string;
  distance: string;
  startLatitude: number;
  startLongitude: number;
  endLatitude: number;
  endLongitude: number;
  routeDescription: string;
}
```
