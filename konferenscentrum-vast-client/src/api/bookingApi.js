import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const bookingApi = {
// GET /api/booking
async getAll() {
  try {
    const response = await api.get("/booking");
    return Array.isArray(response.data) ? response.data : response.data?.data || [];
  } catch (error) {
    throw new Error(
      error.response?.data?.message || "Failed to fetch bookings"
    );
  }
},

  // GET /api/booking/{id}
  async getById(id) {
    try {
      const response = await api.get(`/booking/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Booking not found");
    }
  },

  // GET /api/booking/filter
  async getFiltered(customerId, facilityId, from, to) {
    try {
      const params = new URLSearchParams();
      if (customerId) params.append("customerId", customerId);
      if (facilityId) params.append("facilityId", facilityId);
      if (from) params.append("from", from);
      if (to) params.append("to", to);

      const response = await api.get(`/booking/filter?${params}`);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to fetch filtered bookings"
      );
    }
  },

  // POST /api/booking
  async create(bookingData) {
    try {
      console.log("Sending booking data:", bookingData); // Debug-logg
      const response = await api.post("/booking", bookingData);
      return response.data;
    } catch (error) {
      console.error("Error response:", error.response?.data); // Debug-logg
      throw new Error(
        error.response?.data?.message || "Failed to create booking"
      );
    }
  },

  // POST /api/booking/{id}/confirm
  async confirm(id) {
    try {
      const response = await api.post(`/booking/${id}/confirm`);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to confirm booking"
      );
    }
  },

  // POST /api/booking/{id}/reschedule
  async reschedule(id, startDate, endDate) {
    try {
      const response = await api.post(`/booking/${id}/reschedule`, {
        startDate,
        endDate,
      });
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to reschedule booking"
      );
    }
  },

  // DELETE /api/booking/{id}
  async cancel(id, reason) {
    try {
      await api.delete(`/booking/${id}`, {
        data: { reason },
      });
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to cancel booking"
      );
    }
  },
};
