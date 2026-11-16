using System;

namespace LibraryApi.Entities;

public enum ReservationStatus
{
    Active,     // holding a copy, waiting for pickup
    Fulfilled,  // turned into a loan
    Cancelled,
    Expired
}
