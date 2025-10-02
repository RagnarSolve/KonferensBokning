import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const facilityApi = {
  // GET /api/facility
  async getAll() {
    try {
      const response = await api.get("/facility");
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to fetch facilities"
      );
    }
  },

  // GET /api/facility/active
  async getActive() {
    try {
      const response = await api.get("/facility/active");
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to fetch active facilities"
      );
    }
  },

  // GET /api/facility/{id}
  async getById(id) {
    try {
      const response = await api.get(`/facility/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Facility not found");
    }
  },

  // POST /api/facility
  async create(facilityData) {
    try {
      const response = await api.post("/facility", facilityData);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to create facility"
      );
    }
  },

  // PUT /api/facility/{id}
  async update(id, facilityData) {
    try {
      const response = await api.put(`/facility/${id}`, facilityData);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to update facility"
      );
    }
  },

  // DELETE /api/facility/{id}
  async delete(id) {
    try {
      await api.delete(`/facility/${id}`);
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to delete facility"
      );
    }
  },

  // PATCH /api/facility/{id}/active
  async setActive(id, isActive) {
    try {
      const response = await api.patch(`/facility/${id}/active`, { isActive });
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to update facility status"
      );
    }
  },
};
