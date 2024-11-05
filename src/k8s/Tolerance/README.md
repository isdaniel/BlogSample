# K8S tolerance

Add taint

```
kubectl taint node {node name} memory=stronge:NoSchedule
```

Remove taint

```
kubectl taint node {node name} memory=stronge:NoSchedule-
```

Use `kubectl describe nodes` observe taint information from node machine.

```
kubectl describe nodes {node name}
#Taints: memory=stronge:NoSchedule
```