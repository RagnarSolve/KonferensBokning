import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const bookingContractApi = {
// GET /api/bookingcontract
async getAll() {
  try {
    const response = await api.get("/bookingcontract");
    return Array.isArray(response.data) ? response.data : response.data?.data || [];
  } catch (error) {
    throw new Error(
      error.response?.data?.message || "Failed to fetch contracts"
    );
  }
},

  // GET /api/bookingcontract/{id}
  async getById(id) {
    try {
      const response = await api.get(`/bookingcontract/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Contract not found");
    }
  },

  // GET /api/bookingcontract/booking/{bookingId}
  async getByBookingId(bookingId) {
    try {
      const response = await api.get(`/bookingcontract/booking/${bookingId}`);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Contract not found for booking"
      );
    }
  },

  // POST /api/bookingcontract/booking/{bookingId}
  async createContract(bookingId, contractData) {
    try {
      const response = await api.post(
        `/bookingcontract/booking/${bookingId}`,
        contractData
      );
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to create contract"
      );
    }
  },

  // PATCH /api/bookingcontract/{id}
  async patch(id, contractData) {
    try {
      const response = await api.patch(`/bookingcontract/${id}`, contractData);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to update contract"
      );
    }
  },

  // POST /api/bookingcontract/{id}/send
  async markSent(id) {
    try {
      const response = await api.post(`/bookingcontract/${id}/send`);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to mark contract as sent"
      );
    }
  },

  // POST /api/bookingcontract/{id}/sign
  async markSigned(id) {
    try {
      const response = await api.post(`/bookingcontract/${id}/sign`);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to sign contract"
      );
    }
  },

  // POST /api/bookingcontract/{id}/cancel
  async cancel(id, reason) {
    try {
      const response = await api.post(`/bookingcontract/${id}/cancel`, reason);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to cancel contract"
      );
    }
  },
};
