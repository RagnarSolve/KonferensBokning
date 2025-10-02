import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// GET /api/customer
export const customerApi = {
  async getAll() {
    try {
      const response = await api.get("/customer");
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to fetch customers"
      );
    }
  },

  // GET /api/customer/{id}
  async getById(id) {
    try {
      const response = await api.get(`/customer/${id}`);
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Customer not found");
    }
  },

  // GET /api/customer/by-email?email={email}
  async getByEmail(email) {
    try {
      const response = await api.get(`/customer/by-email?email=${email}`);
      return response.data;
    } catch (error) {
      if (error.response?.status === 404) {
        return null;
      }
      throw new Error(
        error.response?.data?.message || "Failed to fetch customer"
      );
    }
  },

  // POST /api/customer
  async create(customerData) {
    try {
      const response = await api.post("/customer", customerData);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to create customer"
      );
    }
  },

  // PUT /api/customer/{id}
  async update(id, customerData) {
    try {
      const response = await api.put(`/customer/${id}`, customerData);
      return response.data;
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to update customer"
      );
    }
  },

  // DELETE /api/customer/{id}
  async delete(id) {
    try {
      await api.delete(`/customer/${id}`);
    } catch (error) {
      throw new Error(
        error.response?.data?.message || "Failed to delete customer"
      );
    }
  },
};
