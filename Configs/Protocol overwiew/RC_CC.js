Rządania CC do RC
{
	protocol: "route",
	domian: "DomaianA",
	ends:[{
			node:"client1",
			port:"1"
		}, 
		{
			node:"client2",
			port:"1"
		}
	]
}

{
	protocol: "route",
	domian: "DomaianB",
	ends:[{
			node:"node2",
			port:"2"
		}, 
		{
			node:"node4",
			port:"3"
		}
	]
}

Odpowiedzi RC do CC

KLient w innej domenie
{
	protocol: "route",
	ends:[{
			outerDomian: null
			node:"client",
			port:"1"
		}, 
		{
			outerDomian: "B"
			node:"node4",
			port:"3"
		}
	], //To wskazuje że nie dochodzi do michała więc zestawiamy do node4 port 3 i ja potrzebuję info do jakie CC się odezwać  
	steps: [
		{
			domain: null,
			node: "client1",
			ports: [1]
		},
		{
			domain: null,
			node: "node2",
			ports: [1,2]
		},
		{
			domain: null,
			node: "node4",
			ports: [3]
		}
	]
}

KLient w tej samej domenie
{
	protocol: "route",
	ends:[{
			outerDomian: null
			node:"client1",
			port:"1"
		}, 
		{
			outerDomian: null
			node:"client2",
			port:"1"
		}
	], //To wskazuje że nie dochodzi do michała więc zestawiamy do node4 port 3 i ja potrzebuję info do jakie CC się odezwać  
	steps: [
		{
			domain: null,
			node: "client1",
			ports: [1]
		},
		{
			domain: null
			node: "node2",
			ports: [1,2]
		},
		{	domain: "domianB"
			node: "node2",
			ports: [1]
		},
		{	domain: "domianB"
			node: "node3",
			ports: [3]
		},
		{	domain: "domianC"
			node: "node2",
			ports: [1]
		},
		{	domain: "domianC"
			node: "node3",
			ports: [3]
		},
		{
			node: "client2",
			ports: [1]
		}
	]
}