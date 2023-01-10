<script lang="ts">
	export const ssr = false;

	import '../app.scss';
	import NavMenu from './NavMenu.svelte';
    import { HubConnectionBuilder } from "@microsoft/signalr";
	import { onMount } from 'svelte';

	onMount(async () => {
		await import('bootstrap/js/src/collapse.js');

        var connection = new HubConnectionBuilder().withUrl("/api/events").build();

        connection.on("dispatch", function (message: { type: string, body: object }) {
            document.dispatchEvent(new CustomEvent(message.type, { detail: message.body, bubbles: true }));
        });

        await connection.start();
	});
</script>

<div class="page">
	<div class="sidebar">
		<NavMenu />
	</div>

	<main>
		<article class="content px-4">
			<slot />
		</article>
	</main>
</div>
