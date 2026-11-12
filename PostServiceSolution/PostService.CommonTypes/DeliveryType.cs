namespace PostService.CommonTypes;

/// <summary>Дозволені способи доставки. У JSON передаються назвами рядків.</summary>
public enum DeliveryType
{
    /// <summary>Доставка у відділення поштової служби.</summary>
    Department,
    /// <summary>Доставка кур’єром за адресою отримувача.</summary>
    Courier,
    /// <summary>Прискорена доставка кур’єром.</summary>
    ExpressCourier
}
