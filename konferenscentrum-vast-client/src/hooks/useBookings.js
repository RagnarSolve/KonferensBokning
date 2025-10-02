import { useState } from "react";
import { bookingApi } from "../api/bookingApi";

export const useBookings = () => {
  const [bookings, setBookings] = useState([]);
  const [booking, setBooking] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchAllBookings = async () => {
    setLoading(true);
    try {
      const data = await bookingApi.getAll();
      setBookings(data);
    } finally {
      setLoading(false);
    }
  };

  const fetchBookingById = async (id) => {
    setLoading(true);
    try {
      const data = await bookingApi.getById(id);
      setBooking(data);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const fetchFilteredBookings = async (customerId, facilityId, from, to) => {
    setLoading(true);
    try {
      const data = await bookingApi.getFiltered(
        customerId,
        facilityId,
        from,
        to
      );
      setBookings(data);
    } finally {
      setLoading(false);
    }
  };

  const createBooking = async (bookingData) => {
    setLoading(true);
    try {
      const newBooking = await bookingApi.create(bookingData);
      setBookings((prev) => [...prev, newBooking]);
      return newBooking;
    } finally {
      setLoading(false);
    }
  };

  const confirmBooking = async (id) => {
    setLoading(true);
    try {
      const updatedBooking = await bookingApi.confirm(id);
      setBookings((prev) =>
        prev.map((b) => (b.id === id ? updatedBooking : b))
      );
      setBooking(updatedBooking);
      return updatedBooking;
    } finally {
      setLoading(false);
    }
  };

  const rescheduleBooking = async (id, startDate, endDate) => {
    setLoading(true);
    try {
      const updatedBooking = await bookingApi.reschedule(
        id,
        startDate,
        endDate
      );
      setBookings((prev) =>
        prev.map((b) => (b.id === id ? updatedBooking : b))
      );
      setBooking(updatedBooking);
      return updatedBooking;
    } finally {
      setLoading(false);
    }
  };

  const cancelBooking = async (id, reason) => {
    setLoading(true);
    try {
      await bookingApi.cancel(id, reason);
      setBookings((prev) => prev.filter((b) => b.id !== id));
      if (booking?.id === id) {
        setBooking(null);
      }
    } finally {
      setLoading(false);
    }
  };

  return {
    bookings,
    booking,
    loading,
    fetchAllBookings,
    fetchBookingById,
    fetchFilteredBookings,
    createBooking,
    confirmBooking,
    rescheduleBooking,
    cancelBooking,
  };
};
