<script>
	import { onMount } from 'svelte';
	import { search } from '$lib/stores/stores.js';

	let data = {
		id: '',
		address: ''
	};

	let searchValue = '';

	search.subscribe(async (value) => {
		searchValue = value;
	});

	async function updateAddress() {
		await fetch(`/api/customer/address`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				id: data.id,
				address: data.address
			})
		});

		await getCustomer();
	}

	async function getCustomer() {
		const res = await fetch(`/api/customer`);
		data = await res.json();
	}

	onMount(async () => {
		await getCustomer();
	});
</script>

<h5>Profile</h5>

<form>
  <div class="form-group">
    <label for="inputAddress">Address</label>
    <input type="text" class="form-control" bind:value={data.address}>
  </div>
	<button type="button" class="btn btn-outline-secondary btn-sm" on:click={() => updateAddress()}>Update Address</button>
</form>
