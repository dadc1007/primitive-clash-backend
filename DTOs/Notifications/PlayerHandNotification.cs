using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record PlayerHandNotification(List<PlayerCardNotification> Hand, PlayerCardNotification NextCard);