<script>
	import { onMount, onDestroy } from 'svelte';
	import { search } from '$lib/stores/stores.js';

	let data = {
		id: '',
		address: ''
	};

	let searchValue = '';
    let lastEvent = '';
    let error = '';

	search.subscribe(async (value) => {
		searchValue = value;
	});

    function onCustomerAddressUpdated(e) {
        lastEvent = new Date().toLocaleString('en-US');
    }

	async function updateAddress() {
		const res = await fetch(`/api/customer/address`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				id: data.id,
				address: data.address
			})
		});

        if (res.ok)
		    data = await res.json();
        else
            error = new Date().toLocaleString('en-US');
	}

	async function getCustomer() {
		const res = await fetch(`/api/customer`);
		data = await res.json();
	}

	async function throwError() {
		const res = await fetch(`/api/exception`);

        if (res.ok)
		    data = await res.json();
        else {
            let problemDetails = await res.json();
            error = problemDetails.title;
        }
	}

	onMount(async () => {
		await getCustomer();
        document.addEventListener('ConcordServicing.Data.Messages.CustomerAddressUpdated', (e) => onCustomerAddressUpdated(e.detail));

        return () => {
            document.removeEventListener("ConcordServicing.Data.Messages.CustomerAddressUpdated", (e) => onCustomerAddressUpdated(e.detail));
        }
    });
</script>

<h5>Profile</h5>

<form>
  <div>Event time: {lastEvent}</div>
  <div>Error: {error}</div>
  <div class="form-group">
    <label for="inputAddress">Address</label>
    <input type="text" class="form-control" bind:value={data.address}>
  </div>
	<button type="button" class="btn btn-outline-secondary btn-sm" on:click={() => updateAddress()}>Update Address</button>
	<button type="button" class="btn btn-outline-secondary btn-sm" on:click={() => throwError()}>Throw Error</button>

</form>
