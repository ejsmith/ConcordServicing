<script>
	import { search } from '$lib/stores/stores.js';

	let searchValue = '';

	search.subscribe((value) => {
		searchValue = value;
	});

	function setSearch() {
		search.set(searchValue);
	}
</script>

<nav class="navbar navbar-expand-lg navbar-dark bg-dark">
	<div class="container-fluid">
		<a class="navbar-brand" href="/">
            <img src="logo.png" alt="Concord Servicing" width="150" />
        </a>
		<button
			class="navbar-toggler"
			type="button"
			data-bs-toggle="collapse"
			data-bs-target="#navbarSupportedContent"
			aria-controls="navbarSupportedContent"
			aria-expanded="false"
			aria-label="Toggle navigation"
		>
			<span class="navbar-toggler-icon" />
		</button>
		<div class="collapse navbar-collapse" id="navbarSupportedContent">
			<ul class="navbar-nav me-auto mb-2 mb-lg-0">
				<li class="nav-item">
					<a class="nav-link active" aria-current="page" href="/">Home</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" href="profile">Profile</a>
				</li>
			</ul>
			<form class="d-flex">
				<input
					id="searchValue"
					class="form-control me-2"
					type="search"
					placeholder="Search"
					aria-label="Search"
					bind:value={searchValue}
				/>
				<button class="btn btn-outline-success" on:click={setSearch}>Search</button>
			</form>
		</div>
	</div>
</nav>
